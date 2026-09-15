using Game.Settings;
namespace TrafficLightIntoKorea {
public sealed partial class Settings {
 internal const string SaveGuideText="교차로 지정과 개별 편집 설정은 세이브와 짝을 이루는 외부 JSON 파일에도 저장됩니다. 도시를 백업·공유하거나 다른 PC로 옮길 때 세이브와 해당 JSON을 함께 보관하세요. 파일을 구분하기 어렵다면 아래 폴더 전체를 백업하세요. 다른 PC에도 모드와 필수 에셋이 필요합니다.";
 [SettingsUISection("Other","SaveGuide"),SettingsUIMultilineText] public string SaveGuide=>SaveGuideText;
 [SettingsUISection("Other","SaveGuide"),SettingsUIMultilineText] public string SaveFolder=>System.IO.Path.Combine(UnityEngine.Application.persistentDataPath,"ModsData","TrafficLightIntoKorea","IndividualSignals");
 [SettingsUISection("Troubleshooting","Actions"),SettingsUIHideByCondition(typeof(Settings),nameof(NoSaveProblem))] public string SaveProblem=>IndividualSignalData.SaveProblem;
 public bool NoSaveProblem()=>string.IsNullOrEmpty(IndividualSignalData.SaveProblem);
}
}
