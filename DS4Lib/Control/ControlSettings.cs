namespace DS4Lib.Control
{
    public class ControlSettings
    {
        public DS4Controls control;
        public string extras = "0,0,0,0,0,0,0,0";
        public DS4KeyType keyType = DS4KeyType.None;

        public enum ActionType : byte
        {
            Default,
            Key,
            Button,
            Macro
        };

        public ActionType actionType = ActionType.Default;
        public object action;
        public ActionType shiftActionType = ActionType.Default;
        public object shiftAction;
        public int shiftTrigger;
        public string shiftExtras = "0,0,0,0,0,0,0,0";
        public DS4KeyType shiftKeyType = DS4KeyType.None;

        public ControlSettings(DS4Controls ctrl)
        {
            control = ctrl;
        }

        public void Reset()
        {
            extras = "0,0,0,0,0,0,0,0";
            keyType = DS4KeyType.None;
            actionType = ActionType.Default;
            action = null;
            shiftActionType = ActionType.Default;
            shiftAction = null;
            shiftTrigger = 0;
            shiftExtras = "0,0,0,0,0,0,0,0";
            shiftKeyType = DS4KeyType.None;
        }

        internal void UpdateSettings(bool shift, object act, string exts, DS4KeyType kt, int trigger = 0)
        {
            if (!shift)
            {
                if (act is int || act is ushort)
                    actionType = ActionType.Key;
                else if (act is string || act is X360Controls)
                    actionType = ActionType.Button;
                else if (act is int[])
                    actionType = ActionType.Macro;
                else
                    actionType = ActionType.Default;
                action = act;
                extras = exts;
                keyType = kt;
            }
            else
            {
                if (act is int || act is ushort)
                    shiftActionType = ActionType.Key;
                else if (act is string || act is X360Controls)
                    shiftActionType = ActionType.Button;
                else if (act is int[])
                    shiftActionType = ActionType.Macro;
                else
                    shiftActionType = ActionType.Default;
                shiftAction = act;
                shiftExtras = exts;
                shiftKeyType = kt;
                shiftTrigger = trigger;
            }
        }
    }
}