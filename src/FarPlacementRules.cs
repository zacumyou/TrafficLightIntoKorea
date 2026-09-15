namespace TrafficLightIntoKorea {
internal static class FarPlacementRules{internal static bool Accept(float distance,float startAgreement,float endAgreement,float rise)=>distance>=5f&&distance<=80f&&startAgreement>.9f&&endAgreement>.7f&&System.Math.Abs(rise)<=3f;}
}
