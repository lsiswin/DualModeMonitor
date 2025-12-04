using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorLibrary.HttpService;
using MonitorLibrary.Models;
using MonitorLibrary.Reactive;
using Opc.Ua;
using Opc.Ua.Server;
using OpcUaTempSensorServer.Model;

namespace OpcUaTempSensorServer.Services
{
    /// <summary>
    /// 设备节点管理器 - 动态创建传感器节点
    /// </summary>
    public class DeviceNodeManager : CustomNodeManager2
    {
        /// <summary>
        /// 根设备文件夹节点的状态对象（在地址空间创建时初始化）。
        /// </summary>
        private FolderState _devicesFolder = null!;

        /// <summary>
        /// 存储设备 Id 到设备节点组映射的字典（用于运行时访问各传感器变量节点）。
        /// </summary>
        private readonly Dictionary<int, DeviceNodeInfo> _deviceNodes = new();

        /// <summary>
        /// 日志记录器，用于记录地址空间初始化与运行时信息。
        /// </summary>
        private readonly ReactiveLogger _logger;

        /// <summary>
        /// 使用指定服务器与配置创建 <see cref="DeviceNodeManager"/> 的实例。
        /// </summary>
        /// <param name="server">IServerInternal 实例，表示托管的 OPC UA 服务器。</param>
        /// <param name="configuration">应用程序配置，用于节点管理器初始化。</param>
        /// <param name="logger">可选的日志记录器实例（允许为 null）。</param>

        public DeviceNodeManager(
            IServerInternal server,
            ApplicationConfiguration configuration,
            ReactiveLogger logger
        )
            : base(server, configuration, Namespaces.MonitorServer)
        {
            _logger = logger;
            SystemContext.NodeIdFactory = this;
        }

        /// <summary>
        /// 在地址空间中创建并注册预定义节点。
        /// </summary>
        /// <param name="externalReferences">
        /// 外部引用集合，用于将新创建的节点与服务器地址空间的现有节点关联。
        /// 方法实现应在锁定 <see cref="Lock"/> 后执行以保证线程安全。
        /// </param>
        public override void CreateAddressSpace(
            IDictionary<NodeId, IList<IReference>> externalReferences
        )
        {
            lock (Lock)
            {
                LoadPredefinedNodes(SystemContext, externalReferences);

                // 创建根文件夹
                _devicesFolder = CreateFolder(null!, "Devices", "设备文件夹");
                _devicesFolder.AddReference(
                    ReferenceTypeIds.Organizes,
                    true,
                    ObjectIds.ObjectsFolder
                );
                AddPredefinedNode(SystemContext, _devicesFolder);

                _logger?.LogInformation("OPC UA地址空间初始化完成");
            }
        }

        /// <summary>
        /// 根据设备信息创建节点
        /// </summary>
        /// <param name="references"></param>
        /// <returns></returns>
        public void CreateDeviceNode(DeviceInfo device, List<DataPointInfo> dataPoints)
        {
            {
                lock (Lock)
                {
                    try
                    {
                        // 如果设备节点已存在，先删除
                        if (_deviceNodes.ContainsKey(device.Id))
                        {
                            RemoveDeviceNode(device.Id);
                        }
                        //创建设备文件夹节点
                        var deviceFolder = CreateFolder(
                            _devicesFolder,
                            device.DeviceCode,
                            device.Name
                        );
                        deviceFolder.Description = new LocalizedText(
                            $"{device.Name} - {device.Location}"
                        );
                        var deviceNodeInfo = new DeviceNodeInfo
                        {
                            DeviceId = device.Id,
                            DeviceFolder = deviceFolder,
                            DataPointNodes = new Dictionary<int, BaseDataVariableState>(),
                        };
                        // 为每个数据点创建变量节点
                        foreach (var dataPoint in dataPoints)
                        {
                            var variableNode = CreateDataPointVariable(
                                deviceFolder,
                                dataPoint.Code,
                                dataPoint.Name,
                                dataPoint.Unit
                            );
                            deviceNodeInfo.DataPointNodes[dataPoint.Id] = variableNode;
                            _logger?.LogDebug($"创建数据点节点: {device.Name}.{dataPoint.Name}");
                        }
                        _deviceNodes[device.Id] = deviceNodeInfo;

                        _logger?.LogInformation(
                            $"创建设备节点: {device.Name}, 数据点数量: {dataPoints.Count}"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"创建设备节点失败: {device.Name}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// 创建数据点变量节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        private BaseDataVariableState CreateDataPointVariable(
            NodeState parent,
            string name,
            string displayName,
            string unit
        )
        {
            var variable = new BaseDataVariableState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                NodeId = new NodeId($"{parent.BrowseName.Name}_{name}", NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = new LocalizedText($"{displayName} ({unit})"),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                DataType = DataTypeIds.Double,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false,
                Value = 0.0,
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow,
            };
            // 添加工程单位属性
            var engineeringUnits = new PropertyState<EUInformation>(variable)
            {
                SymbolicName = "EngineeringUnits",
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                TypeDefinitionId = VariableTypeIds.PropertyType,
                NodeId = new NodeId($"{variable.NodeId}_EU", NamespaceIndex),
                BrowseName = new QualifiedName("EngineeringUnits", NamespaceIndex),
                DisplayName = new LocalizedText("Engineering Units"),
                DataType = DataTypeIds.EUInformation,
                ValueRank = ValueRanks.Scalar,
                Value = new EUInformation(unit, unit, "http://monitor.com"),
            };
            variable.AddChild(engineeringUnits);
            if (parent != null)
            {
                parent.AddChild(variable);
            }
            AddPredefinedNode(SystemContext, variable);

            return variable;
        }

        /// <summary>
        /// 更新数据点的值
        /// </summary>
        public void UpdateDataPointValue(
            int deviceId,
            int dataPointId,
            double value,
            DateTime timestamp
        )
        {
            lock (Lock)
            {
                try
                {
                    if (_deviceNodes.TryGetValue(deviceId, out var deviceNode))
                    {
                        if (
                            deviceNode.DataPointNodes.TryGetValue(dataPointId, out var variableNode)
                        )
                        {
                            variableNode.Value = value;
                            variableNode.Timestamp = timestamp;
                            variableNode.StatusCode = StatusCodes.Good;
                            variableNode.ClearChangeMasks(SystemContext, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(
                        $"更新数据点值失败: DeviceId={deviceId}, DataPointId={dataPointId}",
                        ex
                    );
                }
            }
        }

        /// <summary>
        /// 批量更新设备的所有数据点
        /// </summary>
        public void UpdateDeviceData(
            int deviceId,
            Dictionary<int, (double value, DateTime timestamp)> dataPoints
        )
        {
            lock (Lock)
            {
                foreach (var kvp in dataPoints)
                {
                    UpdateDataPointValue(deviceId, kvp.Key, kvp.Value.value, kvp.Value.timestamp);
                }
            }
        }

        /// <summary>
        /// 移除设备节点
        /// </summary>
        public void RemoveDeviceNode(int deviceId)
        {
            lock (Lock)
            {
                if (_deviceNodes.TryGetValue(deviceId, out var deviceNode))
                {
                    try
                    {
                        // 移除所有数据点节点
                        foreach (var dataPointNode in deviceNode.DataPointNodes.Values)
                        {
                            RemovePredefinedNode(
                                SystemContext,
                                dataPointNode,
                                new List<LocalReference>()
                            );
                        }

                        // 移除设备文件夹
                        RemovePredefinedNode(
                            SystemContext,
                            deviceNode.DeviceFolder,
                            new List<LocalReference>()
                        );

                        _deviceNodes.Remove(deviceId);

                        _logger?.LogInformation($"移除设备节点: DeviceId={deviceId}");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"移除设备节点失败: DeviceId={deviceId}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个 <see cref="FolderState"/> 对象并进行基础属性初始化。
        /// </summary>
        /// <param name="parent">父节点状态；如果不为 null，则会将新文件夹添加为父节点的子节点。</param>
        /// <param name="name">节点的内部名称（用于 SymbolicName、NodeId 与 BrowseName）。</param>
        /// <param name="displayName">节点的显示名称（用于用户界面呈现）。</param>
        /// <returns>已初始化但尚未注册到地址空间的 <see cref="FolderState"/> 实例。</returns>
        /// <remarks>
        /// - 使用 <see cref="NamespaceIndex"/> 为 NodeId 与 BrowseName 指定命名空间索引。
        /// - 如果传入的 <paramref name="parent"/> 非空，则会调用 <see cref="NodeState.AddChild(NodeState)"/> 将其作为子节点添加。
        /// - 此方法不会自动调用 <see cref="AddPredefinedNode(ISystemContext, NodeState)"/>，调用方负责将返回节点添加到地址空间。
        /// </remarks>
        private FolderState CreateFolder(NodeState parent, string name, string displayName)
        {
            var folder = new FolderState(parent)
            {
                SymbolicName = name,
                ReferenceTypeId = ReferenceTypes.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId(name, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = new LocalizedText(displayName),
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None,
            };
            if (parent != null)
            {
                parent.AddChild(folder);
            }
            return folder;
        }

        /// <summary>
        /// 获取所有设备节点信息
        /// </summary>
        public Dictionary<int, DeviceNodeInfo> GetAllDeviceNodes()
        {
            return new Dictionary<int, DeviceNodeInfo>(_deviceNodes);
        }
    }

    /// <summary>
    /// 命名空间
    /// </summary>
    public static class Namespaces
    {
        public const string MonitorServer = "http://monitor.com/opcua";
    }
}
