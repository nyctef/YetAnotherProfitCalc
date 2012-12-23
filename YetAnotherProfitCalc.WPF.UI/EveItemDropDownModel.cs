using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YetAnotherProfitCalc.WPF.UI
{
    [DebuggerDisplayAttribute("{ItemName}|{TypeID}")]
	struct EveItem
	{
		public string ItemName { get; private set; }
        public TypeID TypeID { get; private set; }

		public EveItem(string itemName, TypeID typeId) : this()
		{
			ItemName = itemName;
			TypeID = typeId;
		}
	}

	class EveItemDropDownModel : NotifyPropertyChangedBase
	{
		/// <summary>
		/// delay in ms between changes in the dropdown and updating the potential completion list
		/// </summary>
		private readonly int m_Delayms;

		/// <summary>
		/// Number of characters that must be entered in the dropdown before it starts searching for items
		/// </summary>
		private readonly int m_MinCharacters;

        private bool m_DropdownShouldBeOpen;
        private EveItem m_SelectedItem;

        public ObservableCollection<EveItem> Items { get; private set; }

        public bool DropdownShouldBeOpen { get { return m_DropdownShouldBeOpen; } set { m_DropdownShouldBeOpen = value; } }

        public EveItem SelectedItem 
        { 
            get { return m_SelectedItem; } 
            set 
            { 
                m_SelectedItem = value;
                FirePropertyChanged("SelectedName");
                FirePropertyChanged("SelectedID");
            } 
        }

        public string SelectedName
        {
            get
            {
                var item = SelectedItem;
                return !item.Equals(default(EveItem)) ? item.ItemName : null;
            }
        }

        public TypeID SelectedID
        {
            get
            {
                var item = SelectedItem;
                return !item.Equals(default(EveItem)) ? item.TypeID : new TypeID(0);
            }
        }

        public static IEnumerable<EveItem> Completions(string input)
        {
            var completions = CommonQueries.GetTypesWithNamesLike(input + "%").ToList();
            if (completions.Count < 100) 
            {
                completions.AddRange(CommonQueries.GetTypesWithNamesLike("%"+input+"%").Where(item => !completions.Contains(item)));
            }

            return completions.Select(res => new EveItem(res.Item1, res.Item2));
        }

		private string m_Input;
		public string Input
		{
			get { return m_Input; }
			set 
            {
                if (m_Input == value) return;
                m_Input = value;
                var updateNeeded = value != null && value.Length >= m_MinCharacters;

                if (updateNeeded)
                {
                    UpdateItems(Completions(value));
                }
                UpdateProperty("DropdownShouldBeOpen", ref m_DropdownShouldBeOpen, updateNeeded);
            }
		}

		public EveItemDropDownModel(int delayms = 0, int minCharacters = 3)
		{
			m_Delayms = delayms;
			m_MinCharacters = minCharacters;
			Items = new ObservableCollection<EveItem>();
		}

		private void UpdateItems(IEnumerable<EveItem> items)
		{
			Items.Clear();
			foreach (var item in items)
			{
				Items.Add(item);
			}
		}
	}

	/// <summary>
	/// An actor which runs on a background thread and delivers completion data to <see cref="EveItemDropDownModel"/>
	/// </summary>
	class EveItemCompletionFetcher : DisposableBase
	{
		private readonly object m_Sync = new object();

		private DateTime m_TimeToNextCheck;

		//private bool m_IsEnabled;

		private Task m_CurrentTask;

		private CancellationTokenSource cts = new CancellationTokenSource();
		
		public void SetNextUpdate(DateTime time, Action<IEnumerable<EveItem>> updateCallback)
		{
			lock (m_Sync)
			{
				m_TimeToNextCheck = time;
				//m_IsEnabled = true;

				if (m_CurrentTask != null)
				{
					cts.Cancel();
				}

				cts = new CancellationTokenSource();
				m_CurrentTask = Task.Delay(m_TimeToNextCheck - DateTime.Now, cts.Token)
					.ContinueWith(task => GetItems())
					.ContinueWith(task => updateCallback(task.Result));
				m_CurrentTask.Start();
			}
		}

		private IEnumerable<EveItem> GetItems()
		{
			return new[]
			{
				new EveItem("foo", new TypeID(1)), 
				new EveItem("bar", new TypeID(2)), 
				new EveItem("baz", new TypeID(3)), 
			};
		}

		public void CancelUpdate()
		{
			lock (m_Sync)
			{
				//m_IsEnabled = false;	
				cts.Cancel();
			}
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    cts.Dispose();
                    cts = null;
                }
                catch { }
            }
            base.Dispose(disposing);
        }
	}
}
