﻿using System.Text;
using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class MemoGui : GuiItem
    {
        private ScrollBar _scrollBar;
        private TextGui _text;

        public MemoGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "panel4.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f,
                0f);

            _text = new TextGui(this, 
                new Vector2(90, 155),
                150,
                180,
                Globals.FontSize10,
                1,
                1,
                "",
                new Color(40, 25, 15)*0.8f);

            var slideTexture = Utils.GetAsf(@"asf\ui\option\", "slidebtn.asf");
            var slideBaseTexture = new Texture(slideTexture);
            var slideClikedTexture = new Texture(slideTexture, 0, 1);
            var slideButton = new GuiItem(this,
                Vector2.Zero,
                slideBaseTexture.Width,
                slideBaseTexture.Height,
                slideBaseTexture,
                null,
                slideClikedTexture,
                null,
                Utils.GetSoundEffect("界-大按钮.wav"));
            _scrollBar = new ScrollBar(this,
                28,
                190,
                null,
                ScrollBar.ScrollBarType.Vertical,
                slideButton,
                new Vector2(295, 108),
                0,
                1,
                0);
            _scrollBar.Scrolled += (arg1, arg2) => SetTextShow(arg2.Value);

            MouseScrollUp += (arg1, arg2) => _scrollBar.Value -= 1;
            MouseScrollDown += (arg1, arg2) => _scrollBar.Value += 1;

            IsShow = false;
        }

        public void UpdateTextShow()
        {
            var count = MemoListManager.GetCount();
            _scrollBar.MaxValue = count - 1;
            SetTextShow(_scrollBar.Value);
        }

        public void SetTextShow(int indexBegin)
        {
            var text = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                text.Append(MemoListManager.GetString(indexBegin + i) + "\n");
            }
            _text.Text = text.ToString();
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow)return;
            base.Update(gameTime);
            _text.Update(gameTime);
            _scrollBar.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _text.Draw(spriteBatch);
            _scrollBar.Draw(spriteBatch);
        }
    }
}