using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Hitorus.Web.Components.Dialogs {
    public partial class TextFieldDialog : ComponentBase {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public string ActionText { get; set; } = null!;
        [Parameter] public string Text { get; set; } = "";

        private readonly List<Func<string, string?>> _validators = [IsEmpty];
        private string _errorMessage = "";
        private bool _showErrorMessage = false;

        public void AddValidators(params IEnumerable<Func<string, string?>> funcs) {
            foreach (Func<string, string?> func in funcs) {
                _validators.Add(func);
            }
        }

        private static string? IsEmpty(string value) {
            if (value.Length == 0) {
                return "Value cannot be empty.";
            }
            return null;
        }

        private void OnTextChanged(string value) {
            Text = value;
            _showErrorMessage = false;
        }

        public void ExecuteAction() {
            foreach (Func<string, string?> validate in _validators) {
                string? error = validate(Text);
                if (error != null) {
                    _errorMessage = error;
                    _showErrorMessage = true;
                    return;
                }
            }
            MudDialog.Close(DialogResult.Ok(Text));
        }

        private void OnKeyDown(KeyboardEventArgs args) {
            if (args.Key == "Enter") {
                ExecuteAction();
            }
        }
    }
}
