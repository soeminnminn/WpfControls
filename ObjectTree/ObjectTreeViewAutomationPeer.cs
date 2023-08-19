using System;
using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace ObjectTree
{
    /// <summary>
    /// Exposes <see cref="T:ObjectTreeView"/> types to UI Automation.
    /// </summary>
    internal class ObjectTreeViewAutomationPeer : ListBoxAutomationPeer
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTreeViewAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public ObjectTreeViewAutomationPeer(ObjectTreeView owner)
            : base(owner)
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the name of the <see cref="T:ObjectTreeView" /> that is associated with this <see cref="T:ObjectTreeViewAutomationPeer" />. 
        /// This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.
        /// </summary>
        /// <returns>A string that contains "ListBox".</returns>
        protected override string GetClassNameCore()
        {
            return nameof(ObjectTreeView);
        }

        /// <summary>
        /// Gets the collection of child elements of the <see cref="T:System.Windows.Controls.ItemsControl" /> 
        /// that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" />. 
        /// This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.
        /// </summary>
        /// <returns>The collection of child elements.</returns>
        protected override List<AutomationPeer> GetChildrenCore()
        {
            return base.GetChildrenCore();
        }
        #endregion
    }
}
