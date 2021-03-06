﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CXFixedLengthFile.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FixedLengthFieldAttribute : Attribute
    {
        private int _order;

        public FixedLengthFieldAttribute(int order)
        {
            _order = order;
        }

        public int GetOrder()
        {
            return _order;
        }
    }
}
