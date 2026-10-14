using UnityEngine;

public interface IToken
{
    Character Host { get; }
    void Attach(Character host);
    string TokenType { get; }
    int Strength { get; }
    int Duration { get; }
    bool IsExpired { get; }
    void Tick(IDamagable target);
    bool Merge(IToken other);
    void OnApply();
    void OnRemove();
    string Name();
    string Description();
    Sprite TokenImage();
}
