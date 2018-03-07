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

using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals
{
    internal class DataGridRowGroupInfo
    {
        public DataGridRowGroupInfo(
#if FEATURE_COLLECTIONVIEWGROUP
            CollectionViewGroup collectionViewGroup,
#endif
            Visibility visibility,
            int level,
            int slot,
            int lastSubItemSlot)
        {
#if FEATURE_COLLECTIONVIEWGROUP
            this.CollectionViewGroup = collectionViewGroup;
#endif
            this.Visibility = visibility;
            this.Level = level;
            this.Slot = slot;
            this.LastSubItemSlot = lastSubItemSlot;
        }

#if FEATURE_COLLECTIONVIEWGROUP
        public CollectionViewGroup CollectionViewGroup
        {
            get;
            private set;
        }
#endif

        public int LastSubItemSlot
        {
            get;
            set;
        }

        public int Level
        {
            get;
            private set;
        }

        public int Slot
        {
            get;
            set;
        }

        public Visibility Visibility
        {
            get;
            set;
        }
    }
}
