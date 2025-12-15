[System.Serializable]
public class SoulInstance
{
    public SoulInstance(SoulData data)
    {
        this.data = data;
    }
    public SoulData data;  // SO 원본 참조
    public int stack = 1;      // 이 소울의 현재 스택

    public string GetEffectText()
    {
        int value = data.GetValue();
        if(value == -1) return null;
        else if(value == 0) return $"{data.soulEffectText}{data.soulEffectText2}";
        else return $"{data.soulEffectText}{value*stack}{data.soulEffectText2}";
    }
}
