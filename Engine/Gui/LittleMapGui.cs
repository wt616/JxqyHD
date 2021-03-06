﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Engine.Gui.Base;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class LittleMapGui : GuiItem
    {
        private GuiItem _leftButton;
        private GuiItem _rightButton;
        private GuiItem _upButton;
        private GuiItem _downButton;
        private GuiItem _closeButton;
        private LineText _mapName;
        private LineText _bottomTip;
        private LineText _messageTip;
        private int _viewBeginX;
        private int _viewBeginY;
        private const int ViewWidth = 320;
        private const int ViewHeight = 240;
        private const int MapViewDrawBeginX = 160;
        private const int MapViewDrawBeginY = 120;
        const int Ratio = 4;
        private Dictionary<string, string> _showNameDictionary;
        private Texture _player;
        private Texture _enemy;
        private Texture _partner;
        private Texture _neutral;

        public override bool IsShow
        {
            get { return base.IsShow; }
            set
            {
                base.IsShow = value;
                if (_messageTip != null)
                {
                    _messageTip.IsShow = false;
                }
                if (value)
                {
                    var texture = Globals.TheMap.LittelMapTexture;
                    if (texture == null)
                    {
                        //Little map texture not exist, can't show little map.
                        base.IsShow = false;
                        return;
                    }

                    ViewBeginX = Globals.TheMap.ViewBeginX / Ratio;
                    ViewBeginY = Globals.TheMap.ViewBeginY / Ratio;
                }
            }
        }

        private Rectangle DrawRegion
        {
            get
            {
                return new Rectangle((int)ScreenPosition.X + MapViewDrawBeginX,
                    (int)ScreenPosition.Y + MapViewDrawBeginY,
                    ViewWidth,
                    ViewHeight);
            }
        }

        private Rectangle ViewRegion
        {
            get
            {
                return new Rectangle(_viewBeginX, 
                    _viewBeginY, 
                    ViewWidth, 
                    ViewHeight);
            }
        }

        public int ViewBeginY
        {
            get { return _viewBeginY; }
            set
            {
                var texture = Globals.TheMap.LittelMapTexture;
                if (texture == null)
                {
                    return;
                }

                _viewBeginY = value;
                if (_viewBeginY + ViewHeight > texture.Height)
                {
                    _viewBeginY = texture.Height - ViewHeight;
                }
                if (_viewBeginY < 0)
                {
                    _viewBeginY = 0;
                }
            }
        }

        public int ViewBeginX
        {
            get { return _viewBeginX; }
            set
            {
                var texture = Globals.TheMap.LittelMapTexture;
                if (texture == null)
                {
                    return;
                }

                _viewBeginX = value;
                if (_viewBeginX + ViewWidth > texture.Width)
                {
                    _viewBeginX = texture.Width - ViewWidth;
                }
                if (_viewBeginX < 0)
                {
                    _viewBeginX = 0;
                }
            }
        }

        public LittleMapGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f,
                0f);
            LoadItems();
            RegisterHadler();
            LoadNameList();
            LoadTexture();

            IsShow = false;
        }

        private void Left()
        {
            ViewBeginX -= 32/Ratio;
        }

        private void Right()
        {
            ViewBeginX += 32/Ratio;
        }

        private void Up()
        {
            ViewBeginY -= 16/Ratio;
        }

        private void Down()
        {
            ViewBeginY += 16/Ratio;
        }

        private void RegisterHadler()
        {
            _leftButton.MouseLeftClicking += (arg1, arg2) => Left();
            _rightButton.MouseLeftClicking += (arg1, arg2) => Right();
            _upButton.MouseLeftClicking += (arg1, arg2) => Up();
            _downButton.MouseLeftClicking += (arg1, arg2) => Down();
            _closeButton.Click += (arg1, arg2) => IsShow = false;
            MouseLeftDown += (object arg1, MouseLeftDownEvent arg2) =>
            {
                if (DrawRegion.Contains(new Point((int)arg2.MouseScreenPosition.X, (int)arg2.MouseScreenPosition.Y)))
                {
                    var position = arg2.MousePosition - new Vector2(MapViewDrawBeginX, MapViewDrawBeginY);
                    position += new Vector2(ViewBeginX, ViewBeginY);
                    position *= Ratio;
                    PathFinder.TemporaryDisableRestrict = true;
                    if (Globals.ThePlayer.canRun(Keyboard.GetState()))
                    {
                        Globals.ThePlayer.RunTo(Map.ToTilePosition(position));
                    }
                    else
                    {
                        Globals.ThePlayer.WalkTo(Map.ToTilePosition(position));
                    }
                    if (Globals.ThePlayer.Path != null)
                    {
                        IsShow = false;
                    }
                    else
                    {
                        _messageTip.IsShow = true;
                    }
                }
            };
        }

        private void LoadTexture()
        {
            _player = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "主角坐标.asf"));
            _enemy = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "敌人坐标.asf"));
            _partner = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "同伴坐标.asf"));
            _neutral = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "路人坐标.asf"));
        }

        private void LoadNameList()
        {
            const string filePath = @"ini\map\mapname.ini";
            try
            {
                _showNameDictionary = new Dictionary<string, string>();
                var data = new FileIniDataParser().ReadFile(filePath, Globals.LocalEncoding);
                var section = Utils.GetFirstSection(data);
                foreach (var keyData in section)
                {
                    _showNameDictionary[keyData.KeyName] = keyData.Value;
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Map name list", filePath, exception);
            }
        }

        private void LoadItems()
        {
            var sound = Utils.GetSoundEffect("界-浏览.wav");
            var asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnleft.asf");
            _leftButton = new GuiItem(this,
                new Vector2(437, 379),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnright.asf");
            _rightButton = new GuiItem(this,
                new Vector2(464, 379),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnup.asf");
            _upButton = new GuiItem(this,
                new Vector2(448, 368),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btndown.asf");
            _downButton = new GuiItem(this,
                new Vector2(448, 395),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnclose.asf");
            _closeButton = new GuiItem(this,
                new Vector2(448, 379),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            _mapName = new LineText(this, 
                new Vector2(210, 92),
                220,
                30,
                LineText.Align.Center, 
                string.Empty,
                new Color(76,56,48) * 0.8f,
                Globals.FontSize12);
            _bottomTip = new LineText(this, 
                new Vector2(160, 370),
                260,
                30,
                LineText.Align.Left,
                "点击小地图进行移动",
                new Color(76, 56, 48) * 0.8f,
                Globals.FontSize10);
            _messageTip = new LineText(this,
                new Vector2(160, 370),
                260,
                30,
                LineText.Align.Right,
                "无法移动到目的地",
                new Color(200, 0, 0) * 0.8f,
                Globals.FontSize10);
            _messageTip.IsShow = false;
        }

        private void DrawMapView(SpriteBatch spriteBatch)
        {
            var viewBeginInWorld = new Vector2(_viewBeginX, _viewBeginY)*Ratio;
            var worldRegion = new Rectangle(_viewBeginX*Ratio,
                _viewBeginY*Ratio,
                ViewWidth*Ratio,
                ViewHeight*Ratio);
            var drawRegion = DrawRegion;
            var drawBeginPosition = new Vector2(drawRegion.X, drawRegion.Y);

            if (Globals.TheMap.LittelMapTexture != null)
            {
                spriteBatch.Draw(Globals.TheMap.LittelMapTexture,
                    new Rectangle((int)drawBeginPosition.X, (int)drawBeginPosition.Y, ViewWidth, ViewHeight),
                    new Rectangle(_viewBeginX, _viewBeginY, ViewWidth, ViewHeight),
                    Color.White);
            }

            var npcs = NpcManager.NpcList;
            foreach (var npc in npcs)
            {
                DrawCharacter(spriteBatch, npc, worldRegion, viewBeginInWorld, drawBeginPosition);
            }
            DrawCharacter(spriteBatch, Globals.ThePlayer, worldRegion, viewBeginInWorld, drawBeginPosition);
        }

        private void DrawCharacter(SpriteBatch spriteBatch, 
            Character character, 
            Rectangle worldRegion,
            Vector2 viewBeginInWorld, 
            Vector2 drawBeginPosition)
        {
            if(character == null) return;

            if (worldRegion.Contains((int)character.PositionInWorld.X, (int)character.PositionInWorld.Y))
            {
                var drawPositon = (character.PositionInWorld - viewBeginInWorld)/4 + drawBeginPosition;
                if (character.IsEnemy)
                {
                    _enemy.Draw(spriteBatch, drawPositon);
                }
                else if (character.IsPartner)
                {
                    _partner.Draw(spriteBatch, drawPositon);
                }
                else if (character.IsPlayer)
                {
                    _player.Draw(spriteBatch, drawPositon);
                }
                else if(character.Kind == (int)Character.CharacterKind.Normal ||
                    character.Kind == (int)Character.CharacterKind.Fighter ||
                    character.Kind == (int)Character.CharacterKind.Eventer)
                {
                    _neutral.Draw(spriteBatch, drawPositon);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            GuiManager.IsMouseStateEated = true;

            var state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Up))
            {
                Up();
            }
            else if (state.IsKeyDown(Keys.Left))
            {
                Left();
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                Right();
            }
            else if (state.IsKeyDown(Keys.Down))
            {
                Down();
            }

            //Buttons
            _leftButton.Update(gameTime);
            _rightButton.Update(gameTime);
            _upButton.Update(gameTime);
            _downButton.Update(gameTime);
            _closeButton.Update(gameTime);

            //Textures
            _player.Update(gameTime);
            _enemy.Update(gameTime);
            _partner.Update(gameTime);
            _neutral.Update(gameTime);

            if (_showNameDictionary.ContainsKey(Globals.TheMap.MapFileNameWithoutExtension))
            {
                _mapName.Text = _showNameDictionary[Globals.TheMap.MapFileNameWithoutExtension];
            }
            else
            {
                _mapName.Text = "无名地图";
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            DrawMapView(spriteBatch);
            base.Draw(spriteBatch);
            _leftButton.Draw(spriteBatch);
            _rightButton.Draw(spriteBatch);
            _upButton.Draw(spriteBatch);
            _downButton.Draw(spriteBatch);
            _closeButton.Draw(spriteBatch);
            _mapName.Draw(spriteBatch);
            _bottomTip.Draw(spriteBatch);
            _messageTip.Draw(spriteBatch);
        }
    }
}