using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScheduleEventSort : IComparer<NPCScheduleEvent>
{
    //时间相同则比较优先级
    public int Compare(NPCScheduleEvent event1, NPCScheduleEvent event2)
    {
        if (event1?.Time == event2?.Time)
        {
            if (event1?.priority < event2?.priority)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        else if (event1?.Time > event2.Time)
        {
            return 1;
        }
        else if (event2?.Time > event1?.Time)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}
