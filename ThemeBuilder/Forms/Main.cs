﻿#region Namespace

using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using VisualPlus.Constants;
using VisualPlus.Enumerators;
using VisualPlus.Events;
using VisualPlus.Structure;
using VisualPlus.Toolkit.Dialogs;

#endregion

namespace ThemeBuilder.Forms
{
    /// <summary>The main.</summary>
    public partial class Main : VisualForm
    {
        #region Variables

        private readonly Theme _theme;
        private bool _saved;

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="Main" /> class.</summary>
        public Main()
        {
            InitializeComponent();

            _theme = new Theme(Themes.Visual);
            LoadTheme(_theme);

            tbName.Text = "UnnamedTheme";
            tbAuthor.Text = "Unknown";

            _saved = true;

            UpdateSelection();
        }

        #endregion

        #region Methods

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>Loads the theme settings.</summary>
        /// <param name="theme">The theme to update with.</param>
        private void LoadTheme(Theme theme)
        {
            if (theme == null)
            {
                throw new NoNullAllowedException(nameof(theme));
            }

            _theme.UpdateTheme(theme.Information, theme.ColorPalette);
            rawText.Text = _theme.RawTheme;

            palettePropertyGrid.SelectedObject = _theme.ColorPalette;
        }

        private void Main_HelpButtonClicked(ControlBoxEventArgs e)
        {
            var link = "https://darkbyte7.github.io/VisualPlus/";

            DialogResult dialogResult = VisualMessageBox.Show("Would you like to visit the website?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                Process.Start(link);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
        }

        /// <summary>Create new theme.</summary>
        private void NewTheme()
        {
            tbPath.Text = string.Empty;

            Theme theme = new Theme(Themes.Visual)
                {
                    Information =
                        {
                            Author = "Unknown",
                            Name = "UnnamedTheme"
                        }
                };

            LoadTheme(theme);
            _saved = false;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_saved)
            {
                DialogResult result = MessageBox.Show(@"Would you like to save the unsaved changed?", Application.ProductName, MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem.PerformClick();
                }
                else if (result == DialogResult.No)
                {
                    NewTheme();
                    return;
                }
                else
                {
                    // Cancel
                }
            }

            NewTheme();
        }

        private void OpenDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tbPath.TextLength > 0)
            {
                if (File.Exists(tbPath.Text))
                {
                    string directory = Path.GetDirectoryName(tbPath.Text);
                    Process.Start(directory);
                }
            }
        }

        private void OpenTemplatesDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = SettingConstants.TemplatesFolder;

            if (path.Length <= 0)
            {
                return;
            }

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = @"Theme File|*.xml";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Theme theme = new Theme(openFileDialog.FileName);

                    if (theme.Information.IsNull)
                    {
                        // Unable to read theme information maybe corrupted.
                        MessageBox.Show($@"Unable to load the theme file.{Environment.NewLine}Detected invalid header.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        tbPath.Text = openFileDialog.FileName;
                    }

                    LoadTheme(theme);
                }
            }
        }

        private void PalettePropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ConsoleEx.WriteDebug($@"Updated property: {e.ChangedItem.Label}.");
            ConsoleEx.WriteDebug($@"Changed {e.OldValue} to new {e.ChangedItem.Value}.");

            Color changedItemValue = (Color)e.ChangedItem.Value;

            if (changedItemValue == Color.Empty)
            {
                return;
            }

            UpdateThemeContents();
        }

        private void PalettePropertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            UpdateSelection();
        }

        /// <summary>Saves the theme to file.</summary>
        private void Save()
        {
            using (StreamWriter _streamWriter = new StreamWriter(tbPath.Text, false))
            {
                _streamWriter.WriteLine(rawText.Text);
                _saved = false;
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"Theme File|*.xml";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tbPath.Text = saveFileDialog.FileName;
                    Save();
                }
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_saved)
            {
                return;
            }

            if (string.IsNullOrEmpty(tbPath.Text))
            {
                saveAsToolStripMenuItem.PerformClick();
            }
            else
            {
                Save();
            }
        }

        private void TbThemeInformation_TextChanged(object sender, EventArgs e)
        {
            UpdateThemeContents();
        }

        /// <summary>Update the color information based on the selection in the properties dialog.</summary>
        private void UpdateSelection()
        {
            if (palettePropertyGrid.SelectedGridItem != null)
            {
                object selectedGridItem = palettePropertyGrid.SelectedGridItem.Value;

                if (selectedGridItem is Color)
                {
                    Color selectedItemColor = (Color)palettePropertyGrid.SelectedGridItem.Value;

                    int alpha = Convert.ToInt32(selectedItemColor.A);
                    int red = Convert.ToInt32(selectedItemColor.R);
                    int green = Convert.ToInt32(selectedItemColor.G);
                    int blue = Convert.ToInt32(selectedItemColor.B);

                    Color fromArgb = Color.FromArgb(alpha, red, green, blue);

                    tilePreview.BackColor = fromArgb;
                    tilePreview.BackColorState.Hover = fromArgb;
                    tilePreview.BackColorState.Pressed = fromArgb;
                    tilePreview.BackColorState.Pressed = fromArgb;

                    ColorPalette palette = (ColorPalette)palettePropertyGrid.SelectedObject;

                    tilePreview.TextStyle.Enabled = palette.TextEnabled;
                    tilePreview.TextStyle.Disabled = palette.TextDisabled;
                    tilePreview.TextStyle.Hover = palette.TextHover;
                    tilePreview.TextStyle.Pressed = palette.TextPressed;
                }
            }
        }

        /// <summary>Update the theme contents.</summary>
        private void UpdateThemeContents()
        {
            ThemeInformation themeInformation = new ThemeInformation
                {
                    Author = tbAuthor.Text,
                    Name = tbName.Text
                };

            rawText.Text = new Theme(themeInformation, _theme.ColorPalette).RawTheme;
            _saved = false;
        }

        #endregion
    }
}