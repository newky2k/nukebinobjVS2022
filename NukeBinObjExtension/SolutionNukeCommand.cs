using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.ProjectSystem.Query;
using Microsoft.VisualStudio.RpcContracts.ProgressReporting;

namespace NukeBinObjExtension
{
    /// <summary>
    /// Command1 handler.
    /// </summary>
    [VisualStudioContribution]
    internal class SolutionNukeCommand : Command
    {
        private readonly TraceSource logger;
        private readonly NukeService _nukeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionNukeCommand"/> class.
        /// </summary>
        /// <param name="traceSource">Trace source instance to utilize.</param>
        public SolutionNukeCommand(TraceSource traceSource, NukeService nukeService)
        {
            // This optional TraceSource can be used for logging in the command. You can use dependency injection to access
            // other services here as well.
            this.logger = Requires.NotNull(traceSource, nameof(traceSource));

            this._nukeService = nukeService;
        }

        /// <inheritdoc />
        public override CommandConfiguration CommandConfiguration => new("%NukeBinObjExtension.SolutionNukeCommand.DisplayName%")
        {
            // Use this object initializer to set optional parameters for the command. The required parameter,
            // displayName, is set above. DisplayName is localized and references an entry in .vsextension\string-resources.json.
            Icon = new(ImageMoniker.KnownValues.CleanData, IconSettings.IconAndText),
            Placements =
            [
                 CommandPlacement.VsctParent(new Guid("{d309f791-903f-11d0-9efc-00a0c911004f}"), id: 537, priority: 0), 
            ],
            EnabledWhen = ActivationConstraint.And(!ActivationConstraint.SolutionState(SolutionState.Building),ActivationConstraint.SolutionState(SolutionState.FullyLoaded)),
        };

        /// <inheritdoc />
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            // Use InitializeAsync for any one-time setup or initialization.
            return base.InitializeAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {

            var solutionQueryResults = await context.Extensibility.Workspaces().QuerySolutionAsync(
                                                                                solution => solution.With(solution => new { solution.BaseName, solution.FileName }),
                                                                                cancellationToken);

            var projects = await context.Extensibility.Workspaces().QueryProjectsAsync(
                                                                project => project.With(p => new { p.Name, p.Path }),
                                                                cancellationToken);

            if (solutionQueryResults.Any())
            {
                var sol = solutionQueryResults.First();

                if (projects.Any())
                {

                    var result = await this.Extensibility.Shell().ShowPromptAsync($"Are you sure you want to delete the bin/obj folders for all {projects.Count()} project(s) in {sol.BaseName}?", PromptOptions.OKCancel, cancellationToken);

                    if (result)
                    {
                        await ProcessProjectsAsync(projects.ToList(), cancellationToken);

                        await this.Extensibility.Shell().ShowPromptAsync("The bin/obj folders have been Nuked!", PromptOptions.OK, cancellationToken);
                    }

                }
            }
        }

        private async Task<ProgressReporter> ProcessProjectsAsync(List<IProjectSnapshot> projects, CancellationToken cancellationToken)
        {
            using ProgressReporter progress = await this.Extensibility.Shell().StartProgressReportingAsync("Nuking Projects", new ProgressReporterOptions(isWorkCancellable: true), cancellationToken);

            try
            {
                for (int i = 0; i < projects.Count; i++)
                {
                    var project = projects[i];

                    progress.Report(CreateProgressStatus(i, projects.Count, $"Nuking: {project.Name}"));
                    progress.CancellationToken.ThrowIfCancellationRequested();

                    var projectPath = project.Path;

                    if (!string.IsNullOrEmpty(projectPath))
                    {
                        await _nukeService.NukeAsync(projectPath);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                //
                Debug.WriteLine(ex);
            }

            return progress;

        }
       
        private static ProgressStatus CreateProgressStatus(int current, int max, string description = "")
        {
            return new ProgressStatus((int)((current / (double)max) * 100), description);
        }
    }

}
