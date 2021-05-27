﻿using Biscuit.Datalog;
using Biscuit.Datalog.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Biscuit.Test.Datalog
{
    [TestClass]
    public class ExpressionTest
    {

        [TestMethod]
        public void testNegate()
        {
            SymbolTable symbols = new SymbolTable();
            symbols.Add("a");
            symbols.Add("b");
            symbols.Add("var");

            Expression e = new Expression(Arrays.asList<Op>(
                    new Op.Value(new ID.Integer(1)),
                    new Op.Value(new ID.Variable(2)),
                    new Op.Binary(Op.BinaryOp.LessThan),
                    new Op.Unary(Op.UnaryOp.Negate)
            ));

            Assert.AreEqual(
                    "! 1 < $var",
                    e.print(symbols).get()
            );

            Dictionary<ulong, ID> variables = new Dictionary<ulong, ID>();
            variables.Add(2L, new ID.Integer(0));

            Assert.AreEqual(
                    new ID.Bool(true),
                    e.evaluate(variables).get()
            );
        }
    }
}