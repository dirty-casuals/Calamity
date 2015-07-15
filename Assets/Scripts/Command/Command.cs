using UnityEngine;
using System.Collections;

public class Command
{
    public virtual void Execute( GameObject controllableObject )
    {
    }

    public virtual void ExecuteWithValue( GameObject controllableObject, float axisValue )
    { 
    }
}