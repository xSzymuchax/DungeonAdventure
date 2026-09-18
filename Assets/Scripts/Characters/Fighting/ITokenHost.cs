using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITokenHost
{
    public void AddToken(IToken token);
    public void TickTokens();
}
