using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GetAttackDirection
{
    public enum AttackDirection
    {
        None = 0,
        Font = 1,
        Side = 2,
        Behind = 3
    }
    
    public static AttackDirection SetAttackDirection(Vector3 currentCharacte, Transform tileChatacter)
    {
        Vector3 forward = tileChatacter.TransformDirection(Vector3.forward);
        Vector3 toOther = currentCharacte - tileChatacter.position;
        float dotProduct = Vector3.Dot(forward, toOther);

        if (dotProduct < -0.75f)
        {
            return AttackDirection.Behind;
        }
        else if (dotProduct > 0.75f)
        {
            return AttackDirection.Font;
        }
        else
        {
            return AttackDirection.Side;
        }
        
    }
}
