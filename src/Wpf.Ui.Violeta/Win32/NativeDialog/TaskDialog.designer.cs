// Copyright 2009-2026 Ookii Dialogs Contributors
//
// Licensed under the BSD 3-Clause License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Wpf.Ui.Violeta.Win32;

partial class TaskDialog
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing"><see langword="true" /> if managed resources should be disposed; otherwise, <see langword="false" />.</param>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
                if (_buttons != null)
                {
                    foreach (TaskDialogButton button in _buttons)
                    {
                        button.Dispose();
                    }
                    _buttons.Clear();
                }
                if (_radioButtons != null)
                {
                    foreach (TaskDialogRadioButton radioButton in _radioButtons)
                    {
                        radioButton.Dispose();
                    }
                    _radioButtons.Clear();
                }
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
    }

    #endregion Component Designer generated code
}
