namespace Reflector.PowerShellLanguage
{
	using System;
	using System.Collections;
	using System.Text;
	using Reflector.CodeModel;
	using Reflector;
	using System.IO;
	using Reflector.CodeModel.Memory;

	internal sealed class VisitorWriter : Visitor
	{
		IFormatter formatter;
		ILanguageWriterConfiguration configuration;

		public VisitorWriter(IFormatter formatter, ILanguageWriterConfiguration configuration)
		{
			this.formatter = formatter;
			this.configuration = configuration;
		}

		private void WriteWhitespace()
		{
			formatter.Write(" ");
		}

		public override void VisitAssembly(IAssembly value)
		{
			if (configuration["ShowNamespaceBody"] == "true")
			{
				base.VisitAssembly(value);
			}
			else
			{
				formatter.Write("# Assembly ");
				formatter.WriteDeclaration(value.Name);
				formatter.WriteProperty("Name", value.ToString());
				formatter.WriteProperty("Location", value.Location);
			}
		}

		public override void VisitNamespace(INamespace value)
		{
			if (configuration["ShowNamespaceBody"] == "true")
			{
				base.VisitNamespace(value);
			}
			else
			{
				formatter.Write("# Namespace ");
				formatter.WriteDeclaration(value.Name);
				formatter.WriteProperty("Assembly", Helper.GetAssemblyReference(value.Types[0]).ToString());
				formatter.WriteProperty("Location", Helper.GetAssemblyReference(value.Types[0]).Resolve().Location);


			}
		}

		public override void VisitModule(IModule value)
		{
			if (configuration["ShowNamespaceBody"] == "true")
			{
				base.VisitModule(value);
			}
			else
			{
				formatter.Write("# Namespace ");
				formatter.WriteDeclaration(value.Name);
				formatter.WriteProperty("Location", value.Location);

				long size = new FileInfo(Environment.ExpandEnvironmentVariables(value.Location)).Length;
				if (size > 1024000)
				{
					formatter.WriteProperty("Size", (size / 1024000).ToString("F") + " Mb");
				}
				else if (size > 1024)
				{
					formatter.WriteProperty("Size", (size / 1024).ToString("F") + " Kb");
				}
				else
				{
					formatter.WriteProperty("Size", size + " Bytes");
				}

				formatter.WriteProperty("Runtime", value.TargetRuntimeVersion);


			}
		}

		public override void VisitTypeDeclaration(ITypeDeclaration value)
		{
			if (configuration["ShowTypeDeclarationBody"] == "true")
			{
				base.VisitTypeDeclaration(value);
			}
			else
			{
				formatter.Write("# Type ");
				formatter.WriteDeclaration(value.Name);
				formatter.WriteProperty("Name", value.Namespace + "." + value.Name);
				formatter.WriteProperty("Assembly", Helper.GetAssemblyReference(value).ToString());
				formatter.WriteProperty("Location", Helper.GetAssemblyReference(value).Resolve().Location);


			}
		}

		public override void VisitStatementCollection(IStatementCollection value)
		{
			foreach (IStatement st in value)
			{
				VisitStatement(st);
				formatter.WriteLine();
			}
		}

		public override void VisitMethodDeclarationCollection(IMethodDeclarationCollection value)
		{
			foreach (IMethodDeclaration decl in value)
			{
				VisitMethodDeclaration(decl);
				formatter.WriteLine();
			}
		}

		public override void VisitMethodDeclaration(IMethodDeclaration value)
		{
			// TODO: do not support instance methods for now.
			if (value.HasThis)
			{
				formatter.WriteLiteral("# Instance methods are not supported at the moment.");
				formatter.WriteLine();
				WriteUnsupported(value.ToString());
				formatter.WriteLiteral("# Rendering as static function. Access to 'this' and 'base' will not work.");
				formatter.WriteLine();
			}

			// TODO: support generic methods?
			if (value.GenericArguments.Count != 0)
			{
				WriteUnsupported(value.ToString());
			}

			formatter.WriteKeyword("function");
			WriteWhitespace();
			formatter.WriteDeclaration(value.Name);
			formatter.WriteLine();

			using (new IndentedCodeBlock(formatter))
			{
				VisitParameterDeclarationCollection(value.Parameters);
				IBlockStatement body = value.Body as IBlockStatement;
				if (body != null) VisitBlockStatement(body);
			}
		}

		public override void VisitMethodReturnStatement(IMethodReturnStatement value)
		{
			formatter.WriteKeyword("return");
			WriteWhitespace();
			VisitExpression(value.Expression);
		}

		public override void VisitMethodReturnType(IMethodReturnType value)
		{
			// Functions in PS don't define a return type.
		}

		public override void VisitParameterDeclarationCollection(IParameterDeclarationCollection value)
		{
			if (value.Count > 0)
			{
				formatter.WriteKeyword("param");
				formatter.Write("(");

				foreach (IParameterDeclaration prm in value)
				{
					if (value.IndexOf(prm) != 0)
						formatter.Write(", ");
					VisitParameterDeclaration(prm);
				}

				formatter.Write(")");
				formatter.WriteLine();
				formatter.WriteLine();
			}
		}

		public override void VisitParameterDeclaration(IParameterDeclaration value)
		{
			formatter.Write("[");
			VisitType(value.ParameterType);
			formatter.Write("] $");
			formatter.Write(value.Name);
		}

		public override void VisitAssignExpression(IAssignExpression value)
		{
			VisitExpression(value.Target);
			formatter.Write(" = ");
			VisitExpression(value.Expression);
		}

		public override void VisitForStatement(IForStatement value)
		{
			formatter.WriteLine();
			formatter.WriteKeyword("for");
			formatter.Write("(");
			VisitStatement(value.Initializer);
			formatter.Write("; ");
			VisitExpression(value.Condition);
			formatter.Write("; ");
			VisitStatement(value.Increment);
			formatter.Write(")");
			formatter.WriteLine();

			using (new IndentedCodeBlock(formatter))
			{
				VisitStatement(value.Body);
			}
		}

		public override void VisitLiteralExpression(ILiteralExpression value)
		{
			if (value.Value is string)
			{
				formatter.WriteLiteral("\"" + value.Value + "\"");
			}
			else if (value.Value is bool)
			{
				formatter.WriteKeyword("$" + value.Value.ToString().ToLower());
			}
			else
			{
				if (value.Value == null)
				{
					formatter.WriteLiteral("$null");
				}
				else
				{
					formatter.Write(value.Value.ToString());
				}
			}
		}

		public override void VisitVariableDeclaration(IVariableDeclaration value)
		{
			formatter.WriteDeclaration("$" + value.Name);
		}

		public override void VisitArrayType(IArrayType type)
		{
			base.VisitArrayType(type);
			formatter.Write("[]");
			for (int i = 0; i < type.Dimensions.Count; i++)
			{
				formatter.Write("[]");
			}
		}

		public override void VisitTypeReference(ITypeReference type)
		{
			base.VisitTypeReference(type);

			ITypeReference owner = type.Owner as ITypeReference;
			// Nested class. Render owner first.
			if (owner != null)
			{
				VisitTypeReference(owner);
			}

			// Nested class will have empty namespace, so it renders ok.
			formatter.WriteReference(
				type.Namespace + "." + type.Name,
				String.Empty,
				type);
		}

		public override void VisitArgumentReferenceExpression(IArgumentReferenceExpression value)
		{
			formatter.Write("$" + value.Parameter.Name);
		}

		public override void VisitBinaryExpression(IBinaryExpression value)
		{
			formatter.Write("(");
			VisitExpression(value.Left);

			switch (value.Operator)
			{
				case BinaryOperator.Add:
					formatter.Write(" + ");
					break;
				case BinaryOperator.BitwiseAnd:
					formatter.Write(" -band ");
					break;
				case BinaryOperator.BitwiseExclusiveOr:
					// How to convert XOR to -band, -bnot y -bor operators
					// ($a -band (-bnot $b)) -bor ((-bnot $a) -band $b)
					formatter.WriteLiteral(" # BinaryOperator.BitwiseExclusiveOr # ");
					break;
				case BinaryOperator.BitwiseOr:
					formatter.Write(" -bor ");
					break;
				case BinaryOperator.BooleanAnd:
					formatter.Write(" -and ");
					break;
				case BinaryOperator.BooleanOr:
					formatter.Write(" -or ");
					break;
				case BinaryOperator.Divide:
					formatter.Write(" % ");
					break;
				case BinaryOperator.GreaterThan:
					formatter.Write(" -gt ");
					break;
				case BinaryOperator.GreaterThanOrEqual:
					formatter.Write(" -ge ");
					break;
				case BinaryOperator.IdentityEquality:
				case BinaryOperator.ValueEquality:
					formatter.Write(" -eq ");
					break;
				case BinaryOperator.IdentityInequality:
				case BinaryOperator.ValueInequality:
					formatter.Write(" -ne ");
					break;
				case BinaryOperator.LessThan:
					formatter.Write(" -lt ");
					break;
				case BinaryOperator.LessThanOrEqual:
					formatter.Write(" -le ");
					break;
				case BinaryOperator.Modulus:
					formatter.WriteLiteral(" # BinaryOperator.Modulus # ");
					break;
				case BinaryOperator.Multiply:
					break;
				case BinaryOperator.ShiftLeft:
					formatter.WriteLiteral(" # BinaryOperator.ShiftLeft # ");
					break;
				case BinaryOperator.ShiftRight:
					formatter.WriteLiteral(" # BinaryOperator.ShiftRight # ");
					break;
				case BinaryOperator.Subtract:
					formatter.Write(" - ");
					break;
				default:
					break;
			}

			VisitExpression(value.Right);
			formatter.Write(")");
		}

		public override void VisitUnaryExpression(IUnaryExpression value)
		{
			switch (value.Operator)
			{
				case UnaryOperator.BitwiseNot:
					formatter.Write("-bnot ");
					VisitExpression(value.Expression);
					break;
				case UnaryOperator.BooleanNot:
				case UnaryOperator.Negate:
					formatter.Write("!");
					VisitExpression(value.Expression);
					break;
				case UnaryOperator.PostDecrement:
					VisitExpression(value.Expression);
					formatter.Write("--");
					break;
				case UnaryOperator.PostIncrement:
					VisitExpression(value.Expression);
					formatter.Write("++");
					break;
				case UnaryOperator.PreDecrement:
					formatter.Write("--");
					VisitExpression(value.Expression);
					break;
				case UnaryOperator.PreIncrement:
					formatter.Write("++");
					VisitExpression(value.Expression);
					break;
				default:
					break;
			}
		}

		public override void VisitVariableReference(IVariableReference value)
		{
			formatter.Write("$" + value.Resolve().Name);
		}

		public override void VisitFieldReference(IFieldReference value)
		{
			formatter.Write(value.Name);
		}

		public override void VisitFieldReferenceExpression(IFieldReferenceExpression value)
		{
			if (value.Field.Resolve().Static)
			{
				formatter.Write("[");
				VisitExpression(value.Target);
				formatter.Write("]");
				formatter.Write("::");
				VisitFieldReference(value.Field);
			}
			else
			{
				formatter.WriteLiteral("# Instance fields are not supported yet.");
				formatter.WriteLine();
				WriteUnsupported(value);
			}
		}

		public override void VisitMethodReferenceExpression(IMethodReferenceExpression value)
		{
			bool isStatic = !value.Method.HasThis;

			if (value.Target is IBinaryExpression)
			{
				formatter.Write("(");
				VisitExpression(value.Target);
				formatter.Write(")");
			}
			else
			{
				if (isStatic)
				{
					formatter.Write("[");
					VisitExpression(value.Target);
					formatter.Write("]");
				}
				else
				{
					VisitExpression(value.Target);
				}
			}

			if (isStatic)
			{
				formatter.Write("::");
			}
			else
			{
				formatter.Write(".");
			}

			VisitMethodReference(value.Method);
		}

		public override void VisitMethodReference(IMethodReference value)
		{
			if (value.GenericArguments.Count > 0)
			{
				MethodReferenceExpression mre = new MethodReferenceExpression();
				mre.Method = value;
				WriteUnsupported(mre);
			}
			else
			{
				// build hint
				TextFormatter hint = new TextFormatter();
				VisitorWriter writer = new VisitorWriter(hint, configuration);

				hint.WriteKeyword("function");
				writer.WriteWhitespace();
				hint.WriteDeclaration(value.Name);
				hint.WriteLine();

				using (new IndentedCodeBlock(hint))
				{
					writer.VisitParameterDeclarationCollection(value.Resolve().Parameters);
				}

				formatter.WriteReference(value.Name, hint.ToString(), value);
			}
		}

		public override void VisitMethodInvokeExpression(IMethodInvokeExpression value)
		{
			VisitExpression(value.Method);
			formatter.Write("(");

			foreach (IExpression arg in value.Arguments)
			{
				if (value.Arguments.IndexOf(arg) != 0)
					formatter.Write(", ");
				VisitExpression(arg);
			}

			formatter.Write(")");
		}

		public override void VisitThisReferenceExpression(IThisReferenceExpression value)
		{
			formatter.Write("$this");
		}

		public override void VisitConditionExpression(IConditionExpression value)
		{
			// c = b != null ? b : "bar";
			// $(if ($b -ne $null) {$b} else {“bar”}) 
			formatter.Write("$(");
			formatter.WriteKeyword("if");
			formatter.Write(" (");
			VisitExpression(value.Condition);
			formatter.Write(") { ");
			VisitExpression(value.Then);
			formatter.Write(" } ");
			if (value.Else != null)
			{
				formatter.WriteKeyword("else");
				formatter.Write(" { ");
				VisitExpression(value.Else);
				formatter.Write(" }");
			}
			formatter.Write(")");
		}

		public override void VisitConditionStatement(IConditionStatement value)
		{
			formatter.WriteKeyword("if");
			WriteWhitespace();
			formatter.Write("(");
			VisitExpression(value.Condition);
			formatter.Write(")");
			formatter.WriteLine();
			using (new IndentedCodeBlock(formatter))
			{
				VisitStatement(value.Then);
			}

			if (value.Else.Statements.Count != 0)
			{
				formatter.WriteKeyword("else");
				formatter.WriteLine();
				using (new IndentedCodeBlock(formatter))
				{
					VisitStatement(value.Else); 
				}
			}
		}

		public override void VisitContinueStatement(IContinueStatement value)
		{
			formatter.WriteKeyword("continue");
		}

		public override void VisitBreakStatement(IBreakStatement value)
		{
			formatter.WriteKeyword("break");
		}

		public override void VisitExpressionCollection(IExpressionCollection value)
		{
			VisitExpressionCollection(value, false);
		}

		private void VisitExpressionCollection(IExpressionCollection collection, bool useColon)
		{
			foreach (IExpression expr in collection)
			{
				if (useColon && collection.IndexOf(expr) != 0)
					formatter.Write(", ");
				VisitExpression(expr);
			}
		}

		public override void VisitArrayCreateExpression(IArrayCreateExpression value)
		{
			// Type of the array is irrelevant in a dynamic language so we don't emit the type.
			//formatter.WriteKeyword("new-object");
			//WriteWhitespace();
			//VisitType(value.Type);

			// TODO: Check if this transformation is correct
			if (value.Initializer != null)
			{
				formatter.Write("$(");
				VisitExpressionCollection(value.Initializer.Expressions, true);
				formatter.Write(")");
			}
			else
			{
				// TODO: according to http://channel9.msdn.com/wiki/default.aspx/Channel9.WindowsPowerShellQuickStart, 
				// @(2) or ,2 	 array of 1 element
				// That implies that the array size has to be the desired # of elements + 1 ??

				formatter.Write("@(");
				VisitExpressionCollection(value.Dimensions, true);
				formatter.Write(")");
			}
		}

		public override void VisitArrayIndexerExpression(IArrayIndexerExpression value)
		{
			VisitExpression(value.Target);
			formatter.Write("[");
			VisitExpressionCollection(value.Indices, true);
			formatter.Write("]");
		}

		public override void VisitCastExpression(ICastExpression value)
		{
			// Casts are not needed in a dynamic language
			VisitExpression(value.Expression);
		}

		public override void VisitSwitchStatement(ISwitchStatement value)
		{
			formatter.WriteLine();
			formatter.WriteKeyword("switch");
			WriteWhitespace();
			formatter.Write("(");
			VisitExpression(value.Expression);
			formatter.Write(")");
			formatter.WriteLine();
			using (new IndentedCodeBlock(formatter))
			{
				foreach (ISwitchCase sc in value.Cases)
				{
					VisitSwitchCase(sc);
				}
			}
			formatter.WriteLine();
		}

		public override void VisitConditionCase(IConditionCase value)
		{
			VisitExpression(value.Condition);
			formatter.WriteLine();

			using (new IndentedCodeBlock(formatter))
			{
				VisitBlockStatement(value.Body);
			}
		}

		public override void VisitDefaultCase(IDefaultCase value)
		{
			formatter.WriteKeyword("default");
			formatter.WriteLine();

			using (new IndentedCodeBlock(formatter))
			{
				VisitBlockStatement(value.Body);
			}

		}

		public override void VisitThrowExceptionStatement(IThrowExceptionStatement value)
		{
			formatter.WriteKeyword("throw");
			WriteWhitespace();
			VisitExpression(value.Expression);
		}

		public override void VisitObjectCreateExpression(IObjectCreateExpression value)
		{
			formatter.WriteKeyword("new-object");
			WriteWhitespace();
			VisitType(value.Constructor.DeclaringType);

			if (value.Arguments.Count > 0)
			{
				formatter.Write("(");
				VisitExpressionCollection(value.Arguments, true);
				formatter.Write(")");
			}
		}

		public override void VisitForEachStatement(IForEachStatement value)
		{
			formatter.WriteLine();
			formatter.WriteKeyword("foreach");
			WriteWhitespace();
			formatter.Write("(");
			VisitVariableDeclaration(value.Variable);
			WriteWhitespace();
			formatter.WriteKeyword("in");
			WriteWhitespace();
			VisitExpression(value.Expression);
			formatter.Write(")");
			formatter.WriteLine();

			using (new IndentedCodeBlock(formatter))
			{
				VisitBlockStatement(value.Body);
			}
		}

		public override void VisitPropertyReferenceExpression(IPropertyReferenceExpression value)
		{
			VisitExpression(value.Target);
			VisitPropertyReference(value.Property);
		}

		public override void VisitPropertyReference(IPropertyReference value)
		{
			formatter.Write(".");
			formatter.WriteReference(value.Name, value.Name, value);
		}

		public override void VisitPropertyIndexerExpression(IPropertyIndexerExpression value)
		{
			VisitExpression(value.Target);
			formatter.Write("[");
			VisitExpressionCollection(value.Indices, true);
			formatter.Write("]");
		}

		public override void VisitDoStatement(IDoStatement value)
		{
			formatter.WriteKeyword("do");
			formatter.WriteLine();
			formatter.Write("{");
			formatter.WriteLine();
			formatter.WriteIndent();
			VisitBlockStatement(value.Body);
			formatter.WriteOutdent();
			formatter.Write("} ");
			formatter.WriteKeyword("until");
			WriteWhitespace();
			formatter.Write("(");
			VisitExpression(value.Condition);
			formatter.Write(")");
			formatter.WriteLine();
		}

		public override void VisitCommentStatement(ICommentStatement value)
		{
			formatter.WriteLiteral("#" + value.Comment.Text);
		}

		public override void VisitTypeOfExpression(ITypeOfExpression value)
		{
			formatter.Write("(");
			formatter.WriteKeyword("typeof");
			formatter.Write(" ");
			VisitType(value.Type);
			formatter.Write(")");
		}

		public override void VisitNullCoalescingExpression(INullCoalescingExpression value)
		{
			// b = a ?? "Foo";
			// transform to:
			// b = $(if (a -neq $null) { "Foo" })

			ConditionExpression condition = new ConditionExpression();
			BinaryExpression notnull = new BinaryExpression();
			notnull.Left = value.Condition;
			notnull.Operator = BinaryOperator.ValueInequality;
			notnull.Right = new LiteralExpression();
			condition.Condition = notnull;
			condition.Then = value.Condition;
			condition.Else = value.Expression;

			VisitExpression(condition);
		}

		// TODO: convert while(...) do {}

		/* ---------------------------------------------------------- */
		/* ---------------------------------------------------------- */
		/* ---------------------------------------------------------- */
		/* ---------------------------------------------------------- */
		/* ---------------------------------------------------------- */

		public override void VisitAddressDereferenceExpression(IAddressDereferenceExpression value)
		{
			WriteUnsupported(value);
		}

		public override void VisitAddressOfExpression(IAddressOfExpression value)
		{
			WriteUnsupported(value);
		}

		public override void VisitAddressOutExpression(IAddressOutExpression value)
		{
			WriteUnsupported(value);
		}

		public override void VisitAddressReferenceExpression(IAddressReferenceExpression value)
		{
			WriteUnsupported(value);
		}

		public override void VisitArgumentListExpression(IArgumentListExpression value)
		{
			WriteUnsupported(value);
		}

		public override void VisitAttachEventStatement(IAttachEventStatement value)
		{
			WriteUnsupported(value);
		}

		public override void VisitBaseReferenceExpression(IBaseReferenceExpression value)
		{
			WriteUnsupported(value);
		}

		public override void VisitCanCastExpression(ICanCastExpression value)
		{
			WriteUnsupported(value);
		}

		public override void VisitCatchClause(ICatchClause value)
		{
			WriteUnsupported(value.ToString());
		}

		public override void VisitDelegateCreateExpression(IDelegateCreateExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitDelegateInvokeExpression(IDelegateInvokeExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitEventReference(IEventReference value)
		{
			WriteUnsupported(value.ToString());

		}

		public override void VisitEventReferenceExpression(IEventReferenceExpression value)
		{
			WriteUnsupported(value);

		}

		//public override IType VisitFunctionPointer(IFunctionPointer type)
		//{
		//    //WriteUnsupportedExpression(value);
		//    return value;
		//}

		//public override IType VisitGenericArgument(IGenericArgument type)
		//{
		//    WriteUnsupportedExpression(value);
		//    return value;
		//}

		public override void VisitGenericDefaultExpression(IGenericDefaultExpression value)
		{
			WriteUnsupported(value);

		}

		//public override IType VisitGenericParameter(IGenericParameter type)
		//{
		//    //WriteUnsupportedExpression(value);
		//    return value;
		//}

		public override void VisitGotoStatement(IGotoStatement value)
		{
			WriteUnsupported(value);

		}

		public override void VisitLabeledStatement(ILabeledStatement value)
		{
			WriteUnsupported(value);

		}

		public override void VisitOptionalModifier(IOptionalModifier type)
		{
			WriteUnsupported(type.ToString());
		}

		public override void VisitPointerType(IPointerType type)
		{
			WriteUnsupported(type.ToString());
		}

		public override void VisitReferenceType(IReferenceType type)
		{
			WriteUnsupported(type.ToString());
		}

		public override void VisitRemoveEventStatement(IRemoveEventStatement value)
		{
			WriteUnsupported(value);

		}

		public override void VisitRequiredModifier(IRequiredModifier type)
		{
			WriteUnsupported(type.ToString());
		}

		public override void VisitSizeOfExpression(ISizeOfExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitSnippetExpression(ISnippetExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitStackAllocateExpression(IStackAllocateExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitTryCastExpression(ITryCastExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitTryCatchFinallyStatement(ITryCatchFinallyStatement value)
		{
			WriteUnsupported(value);

		}

		public override void VisitTypedReferenceCreateExpression(ITypedReferenceCreateExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitTypeOfTypedReferenceExpression(ITypeOfTypedReferenceExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitUsingStatement(IUsingStatement value)
		{
			WriteUnsupported(value);

		}

		public override void VisitValueOfTypedReferenceExpression(IValueOfTypedReferenceExpression value)
		{
			WriteUnsupported(value);

		}

		public override void VisitWhileStatement(IWhileStatement value)
		{
			WriteUnsupported(value);

		}

		private void WriteUnsupported(string value)
		{
			formatter.WriteLiteral("# Unsupported expression:");
			formatter.WriteLine();
			formatter.WriteLiteral("#" + value);
			formatter.WriteLine();
		}

		private void WriteUnsupported(IExpression value)
		{
			formatter.WriteLiteral("# Unsupported expression " + value.GetType().Name + ":");
			formatter.WriteLine();
			formatter.WriteLiteral("#" + value.ToString());
			formatter.WriteLine();
		}

		private void WriteUnsupported(IStatement value)
		{
			formatter.WriteLiteral("# Unsupported statement " + value.GetType().Name + ":");
			formatter.WriteLine();
			formatter.WriteLiteral("#" + value.ToString());
			formatter.WriteLine();
		}

		private class IndentedCodeBlock : IDisposable
		{
			IFormatter formatter;
			public IndentedCodeBlock(IFormatter formatter)
			{
				this.formatter = formatter;
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
			}

			public void Dispose()
			{
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
			}
		}
	}
}
