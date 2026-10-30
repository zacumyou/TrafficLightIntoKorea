namespace TrafficLightIntoKorea {
internal enum BulkStatus {None,Disabled,Preparing,Applying,NoTargets,Applied,NoCity,Queued,Restoring,Restored}
internal static class BulkStatusText {
 internal static string ApplyConfirmation(bool korean)=>korean?"세이브 파일을 따로 저장하세요\n이 설정은 초당 프레임 드랍을 야기할 수 있으며, 그로 인해 게임 튕김 현상이 나타날 수 있습니다. 백업을 필히 권장합니다":"Save a separate copy of your city.\nThis setting may cause frame rate drops and game crashes. A backup is strongly recommended.";
 internal static string RestoreConfirmation(bool korean)=>korean?"모든 교차로 신호등을 기본으로 돌립니다\n이 설정으로 전부 교체된 후 저장을 하면 다시 세이브 파일을 불러올 때는 그 상태가 유지됩니다. 원치 않는 설정이 되지 않길 바랍니다":"Restore all junction traffic lights to default.\nIf you save after all replacements are complete, this state will be retained when you reload the save. Please make sure this is the state you want.";
 internal static string Format(BulkStatus status,bool korean,int done=0,int total=0){
  switch(status){
   case BulkStatus.Disabled:return korean?"먼저 모드를 활성화하세요":"Enable the mod first";
   case BulkStatus.Preparing:return korean?"한국식 적용 준비 중":"Preparing Korean signals";
   case BulkStatus.Applying:return (korean?"한국식 적용 중: ":"Applying Korean signals: ")+done+" / "+total;
   case BulkStatus.NoTargets:return korean?"0개 적용":"Applied: 0";
   case BulkStatus.Applied:return korean?done+"개 적용":"Applied: "+done;
   case BulkStatus.NoCity:return korean?"도시를 불러온 후 사용하세요":"Load a city first";
   case BulkStatus.Queued:return korean?"기본 복원 요청 접수":"Restore queued";
   case BulkStatus.Restoring:return (korean?"기본 복원 중: ":"Restoring defaults: ")+done+" / "+total;
   case BulkStatus.Restored:return korean?done+"개 복원":"Restored: "+done;
   default:return "";
  }
 }
}
}
