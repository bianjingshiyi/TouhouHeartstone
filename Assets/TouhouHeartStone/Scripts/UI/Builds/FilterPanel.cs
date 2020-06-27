using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    public partial class FilterPanel
    {
        partial void onAwake()
        {
            DropdownCost.onValueChanged.AddListener(onCostFilterChange);
            DropdownRare.onValueChanged.AddListener(onRareFilterChange);
            DropdownCharacter.onValueChanged.AddListener(onCharacterFilterChange);
            DropdownType.onValueChanged.AddListener(onTypeFilterChange);
            InputField.onValueChanged.AddListener(onNameFilterChange);

            DropdownSort.onValueChanged.AddListener(onSortMethodChange);
        }

        void onCostFilterChange(int id)
        {
            
        }

        void onRareFilterChange(int id)
        {

        }

        void onCharacterFilterChange(int id)
        {

        }

        void onTypeFilterChange(int id)
        {

        }

        void onNameFilterChange(string filter)
        {

        }

        void onSortMethodChange(int id)
        {

        }
    }
}
