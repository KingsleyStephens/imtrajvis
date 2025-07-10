using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class PlotEvent
{
    public float time;
    public string eventType;
    public string[] plotActors;
    public string[] eventData;
    public int significance;

    public override string ToString()
    {
        string eventDataString = string.Empty;
        if (eventData != null)
        {
            eventDataString = " ";
            for (int i = 0; i < eventData.Length; ++i)
            {
                eventDataString += eventData[i].ToString() + ((i < eventData.Length - 1) ? "," : "");
            }
        }

        return "At time " + time + 
            ", actor " + plotActors[0] + " " + 
            eventType + 
            ((plotActors.Length > 1) ? (" actor " + plotActors[1]) : "") + 
            eventDataString;
    }

    public PlotEvent(float time, string eventType, string[] plotActors, string[] eventData, int significance)
    {
        this.time = time;
        this.eventType = eventType;
        this.plotActors = plotActors;
        this.eventData = eventData;
        this.significance = significance;
    }
}


[System.Serializable]
public class NarrativeData
{
    public PlotEvent[] plotEvents;

    public void SortByTime()
    {
        List<PlotEvent> peList = new List<PlotEvent>(plotEvents);
        peList.Sort(delegate (PlotEvent x, PlotEvent y)
        {
            return x.time.CompareTo(y.time);
        });
        plotEvents = peList.ToArray();
    }
}
