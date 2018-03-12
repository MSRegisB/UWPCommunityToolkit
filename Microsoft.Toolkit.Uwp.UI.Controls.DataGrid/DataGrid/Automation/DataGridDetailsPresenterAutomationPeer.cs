// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using Microsoft.Toolkit.Uwp.UI.Controls.Primitives;
using Windows.UI.Xaml.Automation.Peers;

namespace Microsoft.Toolkit.Uwp.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for DataGridDetailsPresenter
    /// </summary>
    public class DataGridDetailsPresenterAutomationPeer : FrameworkElementAutomationPeer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridDetailsPresenterAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">DataGridDetailsPresenter</param>
        public DataGridDetailsPresenterAutomationPeer(DataGridDetailsPresenter owner)
            : base(owner)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsControlElementCore()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsContentElementCore()
        {
            return false;
        }
    }
}