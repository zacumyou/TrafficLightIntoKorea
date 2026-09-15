using Game.Tools;using Game.UI.Tooltip;using Game.UI.Localization;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class KoreanJunctionTooltipSystem:TooltipSystemBase {
 private readonly StringTooltip _busy=new StringTooltip{path="tlkBusy",value=LocalizedString.Id("TLK.Tool.Busy")};
 protected override void OnUpdate(){var tools=World.GetExistingSystemManaged<ToolSystem>();if(!(tools?.activeTool is KoreanJunctionTool tool)||tool.Hovered==Entity.Null)return;if(KoreanJunctionSystem.Active?.IsRestoring??false){AddMouseTooltip(_busy);return;}}
}
}