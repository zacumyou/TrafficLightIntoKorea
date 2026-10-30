using Game.Tools;using Game.UI.Tooltip;using Game.UI.Localization;using Unity.Entities;
namespace TrafficLightIntoKorea {
public partial class KoreanJunctionTooltipSystem:TooltipSystemBase {
 private readonly StringTooltip _busy=new StringTooltip{path="tlkBusy",value=LocalizedString.Id("TLK.Tool.Busy")};
 private readonly StringTooltip _creating=new StringTooltip{path="tlkCreating",value=LocalizedString.Id("TLK.Tool.Creating")};
 private readonly StringTooltip _failed=new StringTooltip{path="tlkCreateFailed",value=LocalizedString.Id("TLK.Tool.CreateFailed")};
 protected override void OnUpdate(){var tools=World.GetExistingSystemManaged<ToolSystem>();if(!(tools?.activeTool is KoreanJunctionTool tool)||tool.Hovered==Entity.Null)return;if(KoreanJunctionSystem.Active?.IsRestoring??false){AddMouseTooltip(_busy);return;}var system=KoreanJunctionSystem.Active;if(system?.Creating(tool.Hovered)??false)AddMouseTooltip(_creating);else if(system?.Failed(tool.Hovered)??false)AddMouseTooltip(_failed);}
}
}