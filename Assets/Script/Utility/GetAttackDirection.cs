using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GetAttackDirection
{
    public enum AttackDirection
    {
        None = 0,
        Front = 1,
        RightSide = 2,
        LeftSide = 3,
        Behind = 4
    }
    
    public static AttackDirection SetAttackDirection(Vector3 currentCharacte, Transform tileChatacter)
    {
        Vector3 forward = tileChatacter.TransformDirection(Vector3.forward);
        Vector3 toOther = currentCharacte - tileChatacter.position;

        float dotProduct = Vector3.Dot(forward, toOther);
        Vector3 crossProduct = Vector3.Cross(forward, toOther);

        Debug.Log("SetAttackDirection dotProduct = " + dotProduct);

        if (dotProduct < -0.75f)
        {
            return AttackDirection.Behind;
        }
        else if (dotProduct > 0.75f)
        {
            return AttackDirection.Front; // Corrected "Font" to "Front"
        }
        else
        {
            if (crossProduct.y > 0)
            {
                return AttackDirection.LeftSide;
            }
            else
            {
                return AttackDirection.RightSide;
            }
        }
        /*Vector3 forward = tileChatacter.TransformDirection(Vector3.forward);
        Vector3 toOther = currentCharacte - tileChatacter.position;
        float dotProduct = Vector3.Dot(forward, toOther);
        Debug.Log("SetAttackDirection dotProduct = " + dotProduct);

        if (dotProduct < -0.75f)
        {
            return AttackDirection.Behind;
        }
        else if (dotProduct > 0.75f)
        {
            return AttackDirection.Font;
        }
        else if (dotProduct > 0)
        {
            return AttackDirection.LeftSide;
        }
        else
        {
            return AttackDirection.RightSide;
        }*/
        
    }
}
