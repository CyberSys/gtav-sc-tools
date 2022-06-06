﻿namespace ScTools.ScriptLang.Workspace;

using ScTools.ScriptLang.Ast;
using ScTools.ScriptLang.Semantics;

using System.IO;

public class SourceFile : IDisposable
{
    private record AnalysisResult(DiagnosticsReport Diagnostics, CompilationUnit Ast);

    private bool isDisposed;
    private CancellationTokenSource? analyzeTaskCts;
    private Task<AnalysisResult?>? analyzeTask;

    public Project Project { get; }
    public string Path { get; }
    public string Text { get; private set; }

    public SourceFile(Project project, string filePath, string text)
    {
        Project = project;
        Path = filePath;
        Text = text;
    }

    public void UpdateText(string text)
    {
        Text = text;
        CancelAnalysis();
    }

    public async Task<CompilationUnit?> GetAstAsync(CancellationToken cancellationToken = default)
    {
        var result = await AnalyzeAsync(cancellationToken).ConfigureAwait(false);
        return result?.Ast;
    }

    public async Task<DiagnosticsReport?> GetDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var result = await AnalyzeAsync(cancellationToken).ConfigureAwait(false);
        return result?.Diagnostics;
    }

    private void CancelAnalysis()
    {
        analyzeTaskCts?.Cancel();
        analyzeTaskCts?.Dispose();
        analyzeTaskCts = null;
        analyzeTask = null;
    }

    private Task<AnalysisResult?> AnalyzeAsync(CancellationToken ct)
    {
        if (analyzeTask is not null)
        {
            // check if the task is already running
            return analyzeTask;
        }

        Debug.Assert(analyzeTaskCts is null);
        analyzeTaskCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        analyzeTask = RunAnalysisAsync(analyzeTaskCts.Token);
        return analyzeTask;
    }

    private Task<AnalysisResult?> RunAnalysisAsync(CancellationToken ct)
        => Task.Run(() =>
        {
            try
            {
                var diagnosticsReport = new DiagnosticsReport();
                var lexer = new Lexer(Path, Text, diagnosticsReport);
                var parser = new Parser(lexer, diagnosticsReport);
                var compilationUnit = parser.ParseCompilationUnit();
                ct.ThrowIfCancellationRequested();
                var sema = new SemanticsAnalyzer(diagnosticsReport);
                compilationUnit.Accept(sema);
                ct.ThrowIfCancellationRequested();
                return new AnalysisResult(diagnosticsReport, compilationUnit);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }, ct);

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                CancelAnalysis();
            }
            isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public static async Task<SourceFile> Open(Project project, string filePath, CancellationToken ct = default)
    {
        var text = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
        return new SourceFile(project, filePath, text);
    }
}
