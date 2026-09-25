public interface IToken
{
    string TokenType { get; }
    int Strength { get; }
    int Duration { get; }
    bool IsExpired { get; }
    void Tick(IDamagable target);
}
