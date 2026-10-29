using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.Missions
{
    /// <summary>
    /// Mission status lifecycle.
    /// </summary>
    public enum MissionStatus { Locked, Available, Active, Completed, Failed }

    /// <summary>
    /// Faction alignment of a mission — drives which NPC contacts are involved.
    /// </summary>
    public enum MissionFaction { Police, Gangster, Civilian, Casino, Wildlife, Racing }

    /// <summary>
    /// Interface every mission (story or side-quest) must implement.
    /// </summary>
    public interface IMission
    {
        string        MissionId     { get; }
        string        Title         { get; }
        string        Description   { get; }
        MissionFaction Faction      { get; }
        MissionStatus Status        { get; }

        void Activate();
        void Tick(float deltaTime);   // called by MissionManager every frame
        void Complete();
        void Fail(string reason);

        event Action<IMission>        OnCompleted;
        event Action<IMission,string> OnFailed;
        event Action<IMission>        OnObjectiveUpdated;
    }

    /// <summary>
    /// A single step inside a mission (e.g. "Go to the docks", "Deliver the package").
    /// </summary>
    [Serializable]
    public class MissionObjective
    {
        public string description;
        public bool   isCompleted;
        public bool   isOptional;

        public MissionObjective(string desc, bool optional = false)
        {
            description = desc;
            isCompleted = false;
            isOptional  = optional;
        }
    }

    /// <summary>
    /// Base class for all missions — implements the boilerplate of IMission
    /// so concrete missions only override Activate(), Tick(), and CheckCompletion().
    /// </summary>
    public abstract class MissionBase : IMission
    {
        // --- IMission contract ---
        public abstract string        MissionId  { get; }
        public abstract string        Title      { get; }
        public abstract string        Description{ get; }
        public abstract MissionFaction Faction   { get; }
        public MissionStatus          Status     { get; protected set; } = MissionStatus.Locked;

        public event Action<IMission>         OnCompleted;
        public event Action<IMission, string> OnFailed;
        public event Action<IMission>         OnObjectiveUpdated;

        protected List<MissionObjective> objectives = new List<MissionObjective>();
        public    IReadOnlyList<MissionObjective> Objectives => objectives;

        /// <summary>Current active objective index (for display).</summary>
        public int CurrentObjectiveIndex { get; protected set; } = 0;

        public virtual void Activate()
        {
            Status = MissionStatus.Active;
            CurrentObjectiveIndex = 0;
            OnActivate();
            Debug.Log($"<color=#FFDD44>[Mission]</color> ACTIVATED: {Title}");
        }

        public abstract void Tick(float deltaTime);

        public virtual void Complete()
        {
            if (Status == MissionStatus.Completed) return;
            Status = MissionStatus.Completed;
            Debug.Log($"<color=#00FF88>[Mission]</color> COMPLETED: {Title}");
            OnCompleted?.Invoke(this);
        }

        public virtual void Fail(string reason)
        {
            if (Status == MissionStatus.Failed) return;
            Status = MissionStatus.Failed;
            Debug.LogWarning($"<color=#FF4444>[Mission]</color> FAILED: {Title} — {reason}");
            OnFailed?.Invoke(this, reason);
        }

        protected void AdvanceObjective()
        {
            if (CurrentObjectiveIndex < objectives.Count)
                objectives[CurrentObjectiveIndex].isCompleted = true;
            CurrentObjectiveIndex = Mathf.Min(CurrentObjectiveIndex + 1, objectives.Count);
            OnObjectiveUpdated?.Invoke(this);
        }

        protected void NotifyObjectiveUpdated() => OnObjectiveUpdated?.Invoke(this);

        protected virtual void OnActivate() { }

        // Helpers for world interaction
        protected static Transform GetPlayer()
        {
            var p = UnityEngine.Object.FindObjectOfType<DCultCity.Player.ThirdPersonController>();
            return p != null ? p.transform : null;
        }

        protected static float DistToPlayer(Vector3 pos)
        {
            var p = GetPlayer();
            return p != null ? Vector3.Distance(p.position, pos) : float.MaxValue;
        }
    }
}
