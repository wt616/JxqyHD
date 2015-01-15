﻿using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    public class Npc : Character
    {
        #region public properties
        public override PathFinder.PathType PathType
        {
            get
            {
                if (Kind == (int)CharacterType.Flyer)
                {
                    return Engine.PathFinder.PathType.PathStraightLine;
                }
                else if (base.PathFinder == 1 || IsPartner)
                {
                    return Engine.PathFinder.PathType.PerfectMaxNpcTry;
                }
                else
                {
                    return Engine.PathFinder.PathType.PathOneStep;
                }
            }
        }

        public static bool IsAIDisabled { protected set; get; }
        #endregion

        #region Ctor
        public Npc() { }

        public Npc(string filePath)
            : base(filePath)
        {
            if (LevelIni == null)
            {
                LevelIni = Utils.GetLevelLists(@"ini\level\level-npc.ini");
            }
        }

        public Npc(KeyDataCollection keyDataCollection)
            : base(keyDataCollection)
        {
            if (LevelIni == null)
            {
                LevelIni = Utils.GetLevelLists(@"ini\level\level-npc.ini");
            }
        }
        #endregion Ctor

        private void MoveToPlayer()
        {
            if (!Globals.ThePlayer.IsStanding())
            {
                MoveTo(Globals.ThePlayer.TilePosition);
            }
        }

        /// <summary>
        /// Walk or run to destination.
        /// If distance greater than 5, run to destination.
        /// If distance greater than 2, and is running, run to destination, else walk to destination.
        /// </summary>
        /// <param name="destinationTilePosition">Destination tile position</param>
        public void MoveTo(Vector2 destinationTilePosition)
        {
            var distance = Engine.PathFinder.GetTileDistance(TilePosition,
                destinationTilePosition);
            if (distance > 5)
            {
                RunTo(destinationTilePosition);
            }
            else if (distance > 2)
            {
                if (IsRuning())
                {
                    RunTo(destinationTilePosition);
                }
                else
                {
                    WalkTo(destinationTilePosition);
                }
            }
        }

        public override bool HasObstacle(Vector2 tilePosition)
        {
            if (Kind == (int) CharacterType.Flyer) return false;

            return (NpcManager.IsObstacle(tilePosition) ||
                    ObjManager.IsObstacle(tilePosition) ||
                    Globals.ThePlayer.TilePosition == tilePosition);
        }

        protected override void PlaySoundEffect(SoundEffect soundEffect)
        {
            SoundManager.Play3DSoundOnece(soundEffect,
                PositionInWorld - Globals.ListenerPosition);
        }

        protected override void FollowTargetFinded(bool attackCanReach)
        {
            if (IsAIDisabled)
            {
                CancleAttackTarget();
            }
            else
            {
                if (attackCanReach) Attacking(FollowTarget.TilePosition);
                else WalkTo(FollowTarget.TilePosition);
            }
        }

        protected override void FollowTargetLost()
        {
            //just walk to last find position
            CancleAttackTarget();
            if (IsPartner)
            {
                WalkTo(Globals.ThePlayer.TilePosition);
            }
        }

        public static void DisableAI()
        {
            IsAIDisabled = true;
        }

        public static void EnableAI()
        {
            IsAIDisabled = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsAIDisabled)
            {
                //do nothing
            }
            else
            {
                if (!IsFollowTargetFinded ||
                    FollowTarget == null ||
                    FollowTarget.IsDeathInvoked)
                {
                    if (IsEnemy)
                    {
                        FollowTarget = NpcManager.GetClosedPlayerOrFighterFriend(PositionInWorld);
                    }
                    else if (IsFighterFriend)
                    {
                        FollowTarget = NpcManager.GetClosedEnemy(PositionInWorld);
                        //Fighter friend may be parter
                        if (FollowTarget == null && IsPartner)
                        {
                            //Can't find enemy, move to player
                            MoveToPlayer();
                        }
                    }
                    else if (IsPartner)
                    {
                        MoveToPlayer();
                    }
                }
            }

            //Check FixedPos move path and update
            UpdateMoveAlongFixedPath();

            base.Update(gameTime);
        }
    }
}
