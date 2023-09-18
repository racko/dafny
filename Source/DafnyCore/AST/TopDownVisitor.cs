using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Boogie;

namespace Microsoft.Dafny;

public class TopDownVisitor<State> {
  protected bool preResolve = false;

  public void Visit(Expression expr, State st) {
    Contract.Requires(expr != null);
    if (VisitOneExpr(expr, ref st)) {
      if (preResolve && expr is ConcreteSyntaxExpression concreteSyntaxExpression) {
        concreteSyntaxExpression.PreResolveSubExpressions.ForEach(e => Visit((Expression)e, st));
      } else if (preResolve && expr is QuantifierExpr quantifierExpr) {
        // pre-resolve, split expressions are not children
        quantifierExpr.PreResolveSubExpressions.ForEach(e => Visit(e, st));
      } else {
        // recursively visit all subexpressions and all substatements
        expr.SubExpressions.ForEach(e => Visit(e, st));
      }
      if (expr is StmtExpr) {
        // a StmtExpr also has a substatement
        var e = (StmtExpr)expr;
        Visit(e.S, st);
      }
    }
  }
  public void Visit(Statement stmt, State st) {
    Contract.Requires(stmt != null);
    if (VisitOneStmt(stmt, ref st)) {
      // recursively visit all subexpressions and all substatements
      if (preResolve) {
        stmt.PreResolveSubExpressions.ForEach(e => Visit(e, st));
        stmt.PreResolveSubStatements.ForEach(s => Visit(s, st));
      } else {
        stmt.SubExpressions.ForEach(e => Visit(e, st));
        stmt.SubStatements.ForEach(s => Visit(s, st));
      }
    }
  }
  public void Visit(IEnumerable<Expression> exprs, State st) {
    exprs.ForEach(e => Visit(e, st));
  }
  public void Visit(IEnumerable<Statement> stmts, State st) {
    stmts.ForEach(e => Visit(e, st));
  }
  public void Visit(AttributedExpression expr, State st) {
    Visit(expr.E, st);
  }
  public void Visit(FrameExpression expr, State st) {
    Visit(expr.E, st);
  }
  public void Visit(IEnumerable<AttributedExpression> exprs, State st) {
    exprs.ForEach(e => Visit(e, st));
  }
  public void Visit(IEnumerable<FrameExpression> exprs, State st) {
    exprs.ForEach(e => Visit(e, st));
  }
  public void Visit(ICallable decl, State st) {
    if (decl is Function) {
      Visit((Function)decl, st);
    } else if (decl is Method) {
      Visit((Method)decl, st);
    }
    //TODO More?
  }
  public virtual void Visit(Method method, State st) {
    Visit(method.Ens, st);
    Visit(method.Req, st);
    Visit(method.Reads, st);
    Visit(method.Mod.Expressions, st);
    Visit(method.Decreases.Expressions, st);
    if (method.Body != null) { Visit(method.Body, st); }
    //TODO More?
  }
  public virtual void Visit(Function function, State st) {
    Visit(function.Ens, st);
    Visit(function.Req, st);
    Visit(function.Reads, st);
    Visit(function.Decreases.Expressions, st);
    if (function.Body != null) { Visit(function.Body, st); }
    //TODO More?
  }
  /// <summary>
  /// Visit one expression proper.  This method is invoked before it is invoked on the
  /// sub-parts (subexpressions and substatements).  A return value of "true" says to
  /// continue invoking the method on sub-parts, whereas "false" says not to do so.
  /// The on-return value of "st" is the value that is passed to sub-parts.
  /// </summary>
  protected virtual bool VisitOneExpr(Expression expr, ref State st) {
    Contract.Requires(expr != null);
    return true;  // by default, visit the sub-parts with the same "st"
  }

  /// <summary>
  /// Visit one statement proper.  For the rest of the description of what this method
  /// does, see VisitOneExpr.
  /// </summary>
  protected virtual bool VisitOneStmt(Statement stmt, ref State st) {
    Contract.Requires(stmt != null);
    return true;  // by default, visit the sub-parts with the same "st"
  }
}