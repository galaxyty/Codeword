public enum ECard
{
    Red = 0,            // 레드.
    Blue,               // 파랑.
    Neutral,            // 중립.
    Joker,              // 조커.
    Basic               // 기본 카드.
}

public class Card
{
    public int Index;
    public string Name;
    public ECard Type;
    public bool IsOpen;
    public bool IsToggle;
}