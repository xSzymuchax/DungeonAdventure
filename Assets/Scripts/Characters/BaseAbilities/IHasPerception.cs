using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasPerception
{
    public int WakeUpRange { get; }
    public int ViewRange { get; }
    public int DetectionRange { get; }
}
