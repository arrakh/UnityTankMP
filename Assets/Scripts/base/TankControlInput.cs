using UnityEngine;
using System;

namespace UniTank
{
    public class TankControlInput : TankController
    {
        public bool accelerateForward;
        public bool accelerateReverse;
        public bool turnLeft;
        public bool turnRight;
        public bool fire;
        public bool gameMenu;
        public Action OnAim;
        public Action OnFire;
        public Action OnGameMenu;
    }
}