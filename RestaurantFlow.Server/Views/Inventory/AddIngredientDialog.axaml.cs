using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RestaurantFlow.Server.Views.Inventory
{
    public partial class AddIngredientDialog : UserControl
    {
        public AddIngredientDialog()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}