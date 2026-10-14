using System;

public abstract class Token
{
    public Character Host { get; private set; }

    public void Attach(Character host)
    {
        Host = host;
    }

    public virtual void OnApply()
    {
    }

    public virtual void OnRemove()
    {
    }

    protected void DropIfExpired()
    {
        if (Host == null || this is not IToken token || !token.IsExpired)
            return;

        Host.RemoveToken(token);
    }

    protected void RemoveTokens(Func<IToken, bool> match)
    {
        if (Host == null || match == null)
            return;

        bool removed = true;
        while (removed)
        {
            removed = false;
            var active = Host.ActiveTokens;
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i] == null || !match(active[i]))
                    continue;

                Host.RemoveToken(active[i]);
                removed = true;
                break;
            }
        }
    }
}
