﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.GameHelpers;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Configuration.Timeline.TimelineCondition;

internal class ObjectGetter
{
    public ObjectType Type { get; set; }
    public string DataID { get; set; } = "";
    public JobRole Role { get; set; } = JobRole.None;
    public uint Status { get; set; }
    public float StatusTime { get; set; } = 5;
    public Vector2 TimeDuration { get; set; } = new(0, 2);
    public string VfxPath { get; set; } = string.Empty;
    public ushort ObjectEffect1 { get; set; } = 0;
    public ushort ObjectEffect2 { get; set; } = 0;
    public bool CanGet(GameObject obj)
    {
        switch (Type)
        {
            case ObjectType.BattleCharactor:
                if(obj is not BattleChara) return false;
                break;

            case ObjectType.PlayerCharactor:
                if (obj is not PlayerCharacter) return false;
                break;

            case ObjectType.Myself:
                return obj == Player.Object;
        }

        if (!string.IsNullOrEmpty(DataID) && !new Regex(DataID).IsMatch(obj.DataId.ToString("X"))) return false;

        if (Role != JobRole.None && !obj.IsJobCategory(Role)) return false;

        if (Status != 0)
        {
            if (obj is not BattleChara b) return false;
            var status = b.StatusList.FirstOrDefault(s => s.StatusId == Status);
            if (status == null) return false;
            if (status.RemainingTime > StatusTime) return false;
        }

        if (!string.IsNullOrEmpty(VfxPath))
        {
            if (!DataCenter.VfxNewData.Reverse().Any(effect =>
            {
                if (effect.ObjectId != obj.ObjectId) return false;

                var time = effect.TimeDuration.TotalSeconds;

                if (time < TimeDuration.X) return false;
                if (time > TimeDuration.Y) return false;

                if (effect.Path != VfxPath) return false;

                return true;
            })) return false;
        }

        if (ObjectEffect1 != 0 || ObjectEffect2 != 0)
        {
            if (!DataCenter.ObjectEffects.Reverse().Any(effect =>
            {
                if (effect.ObjectId != obj.ObjectId) return false;

                var time = effect.TimeDuration.TotalSeconds;

                if (time < TimeDuration.X) return false;
                if (time > TimeDuration.Y) return false;

                if (effect.Param1 != ObjectEffect1) return false;
                if (effect.Param2 != ObjectEffect2) return false;

                return true;
            })) return false;
        }

        return true;
    }
}

public enum ObjectType : byte
{
    GameObject,
    BattleCharactor,
    PlayerCharactor,
    Myself,
}