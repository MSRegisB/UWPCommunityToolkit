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

using System;
using System.Globalization;

namespace Microsoft.Toolkit.Uwp.UI.Data.Utilities
{
    internal static class CollectionViewsError
    {
        public static class CollectionView
        {
            public static InvalidOperationException EnumeratorVersionChanged()
            {
                return new InvalidOperationException("Collection was modified; enumeration operation cannot execute.");
            }

            public static InvalidOperationException MemberNotAllowedDuringAddOrEdit(string paramName)
            {
                return new InvalidOperationException(Format("'{0}' is not allowed during an AddNew or EditItem transaction.", paramName));
            }

            public static InvalidOperationException NoAccessWhileChangesAreDeferred()
            {
                return new InvalidOperationException("This value cannot be accessed while changes are deferred.");
            }
        }

        internal const string MemberNotAllowedForView = "'{0}' is not allowed for this view.";

        public static class ListCollectionView
        {
            public static InvalidOperationException CollectionChangedOutOfRange()
            {
                return new InvalidOperationException("The collection change is out of bounds of the original collection.");
            }

            public static InvalidOperationException AddedItemNotAtIndex()
            {
                return new InvalidOperationException("The added item is not in the collection.");
            }

            public static InvalidOperationException AddedItemNotInCollection()
            {
                return new InvalidOperationException("The added item is not in the collection.");
            }

            public static InvalidOperationException CancelEditNotSupported()
            {
                return new InvalidOperationException("CancelEdit is not supported for the current edit item.");
            }

            public static InvalidOperationException MemberNotAllowedDuringTransaction(string paramName1, string paramName2)
            {
                return new InvalidOperationException(Format("'{0}' is not allowed during a transaction started by '{1}'.", paramName1, paramName2));
            }

            public static InvalidOperationException MemberNotAllowedForView(string paramName)
            {
                return new InvalidOperationException(Format("'{0}' is not allowed for this view.", paramName));
            }
        }

        private static string Format(string formatString, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, formatString, args);
        }
    }
}
