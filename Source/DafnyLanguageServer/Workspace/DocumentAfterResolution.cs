using System;
using System.Collections.Generic;
using Microsoft.Dafny.LanguageServer.Language.Symbols;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Microsoft.Dafny.LanguageServer.Workspace;

public class DocumentAfterResolution : DocumentAfterParsing {
  public DocumentAfterResolution(DocumentTextBuffer textDocumentItem,
    Dafny.Program program,
    IReadOnlyList<Diagnostic> parseAndResolutionDiagnostics,
    SymbolTable symbolTable,
    IReadOnlyList<Diagnostic> ghostDiagnostics) :
    base(textDocumentItem, program, ArraySegment<Diagnostic>.Empty) {
    ParseAndResolutionDiagnostics = parseAndResolutionDiagnostics;
    SymbolTable = symbolTable;
    GhostDiagnostics = ghostDiagnostics;
  }

  public IReadOnlyList<Diagnostic> ParseAndResolutionDiagnostics { get; }
  public SymbolTable SymbolTable { get; }
  public IReadOnlyList<Diagnostic> GhostDiagnostics { get; }

  public override IEnumerable<Diagnostic> Diagnostics => ParseAndResolutionDiagnostics;

  public override IdeState ToIdeState(IdeState previousState) {
    return previousState with {
      TextDocumentItem = TextDocumentItem,
      ImplementationsWereUpdated = false,
      ResolutionDiagnostics = ParseAndResolutionDiagnostics,
      SymbolTable = SymbolTable.Resolved ? SymbolTable : previousState.SymbolTable,
      GhostDiagnostics = GhostDiagnostics
    };
  }
}