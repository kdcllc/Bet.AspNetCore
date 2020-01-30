using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extension methods.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Executes the specified action if the specified <paramref name="condition"/> is <c>true</c> which can be
        /// used to conditionally add to the request execution pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="condition">If set to <c>true</c> the action is executed.</param>
        /// <param name="action">The action used to add to the request execution pipeline.</param>
        /// <returns>The same application builder.</returns>
        public static IApplicationBuilder UseIf(
            this IApplicationBuilder app,
            bool condition,
            Func<IApplicationBuilder, IApplicationBuilder> action)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (condition)
            {
                app = action(app);
            }

            return app;
        }

        /// <summary>
        /// Executes the specified <paramref name="ifAction"/> if the specified <paramref name="condition"/> is
        /// <c>true</c>, otherwise executes the <paramref name="elseAction"/>. This can be used to conditionally add to
        /// the request execution pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="condition">If set to <c>true</c> the <paramref name="ifAction"/> is executed, otherwise the
        /// <paramref name="elseAction"/> is executed.</param>
        /// <param name="ifAction">The action used to add to the request execution pipeline if the condition is
        /// <c>true</c>.</param>
        /// <param name="elseAction">The action used to add to the request execution pipeline if the condition is
        /// <c>false</c>.</param>
        /// <returns>The same application builder.</returns>
        public static IApplicationBuilder UseIfElse(
            this IApplicationBuilder app,
            bool condition,
            Func<IApplicationBuilder, IApplicationBuilder> ifAction,
            Func<IApplicationBuilder, IApplicationBuilder> elseAction)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (ifAction == null)
            {
                throw new ArgumentNullException(nameof(ifAction));
            }

            if (elseAction == null)
            {
                throw new ArgumentNullException(nameof(elseAction));
            }

            return condition ? ifAction(app) : elseAction(app);
        }

        /// <summary>
        /// Executes the specified action using the <see cref="HttpContext"/> to determine if the specified
        /// <paramref name="condition"/> is <c>true</c> which can be used to conditionally add to the request execution
        /// pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="condition">If set to <c>true</c> the action is executed.</param>
        /// <param name="action">The action used to add to the request execution pipeline.</param>
        /// <returns>The same application builder.</returns>
        public static IApplicationBuilder UseIf(
            this IApplicationBuilder app,
            Func<HttpContext, bool> condition,
            Func<IApplicationBuilder, IApplicationBuilder> action)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var builder = app.New();

            action(builder);

            return app.Use(next =>
            {
                builder.Run(next);

                var branch = builder.Build();

                return context =>
                {
                    if (condition(context))
                    {
                        return branch(context);
                    }

                    return next(context);
                };
            });
        }

        /// <summary>
        /// Executes the specified <paramref name="ifAction"/> using the <see cref="HttpContext"/> to determine if the
        /// specified <paramref name="condition"/> is <c>true</c>, otherwise executes the
        /// <paramref name="elseAction"/>. This can be used to conditionally add to the request execution pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="condition">If set to <c>true</c> the <paramref name="ifAction"/> is executed, otherwise the
        /// <paramref name="elseAction"/> is executed.</param>
        /// <param name="ifAction">The action used to add to the request execution pipeline if the condition is
        /// <c>true</c>.</param>
        /// <param name="elseAction">The action used to add to the request execution pipeline if the condition is
        /// <c>false</c>.</param>
        /// <returns>The same application builder.</returns>
        public static IApplicationBuilder UseIfElse(
            this IApplicationBuilder app,
            Func<HttpContext, bool> condition,
            Func<IApplicationBuilder, IApplicationBuilder> ifAction,
            Func<IApplicationBuilder, IApplicationBuilder> elseAction)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (ifAction == null)
            {
                throw new ArgumentNullException(nameof(ifAction));
            }

            if (elseAction == null)
            {
                throw new ArgumentNullException(nameof(elseAction));
            }

            var ifBuilder = app.New();
            var elseBuilder = app.New();

            ifAction(ifBuilder);
            elseAction(elseBuilder);

            return app.Use(next =>
            {
                ifBuilder.Run(next);
                elseBuilder.Run(next);

                var ifBranch = ifBuilder.Build();
                var elseBranch = elseBuilder.Build();

                return context =>
                {
                    if (condition(context))
                    {
                        return ifBranch(context);
                    }
                    else
                    {
                        return elseBranch(context);
                    }
                };
            });
        }

        /// <summary>
        /// Enables static files serving with the default cache expiration of 1 year.
        /// </summary>
        /// <param name="app"></param>
        public static IApplicationBuilder UseStaticFilesWithCache(this IApplicationBuilder app)
        {
            return app.UseStaticFilesWithCache(TimeSpan.FromDays(365));
        }

        /// <summary>
        /// Enables static files serving with the specified expiration.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="cacheExpiration"></param>
        public static IApplicationBuilder UseStaticFilesWithCache(
            this IApplicationBuilder app,
            TimeSpan cacheExpiration)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = $"max-age={cacheExpiration.TotalSeconds.ToString()}";
                    context.Context.Response.Headers[HeaderNames.Expires] = DateTime.UtcNow.Add(cacheExpiration).ToString("R");

                    if (context.Context.Request.Query.ContainsKey("v"))
                    {
                        context.Context.Response.Headers[HeaderNames.CacheControl] += ",immutable";
                    }
                }
            });

            return app;
        }

        /// <summary>
        /// Adds Https redirection if enabled in the configuration.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="sectionName">The name of the section where configuration for this is set.
        /// The default value is "HttpsRedirection".</param>
        /// <returns></returns>
        public static IApplicationBuilder UseOrNotHttpsRedirection(
            this IApplicationBuilder app,
            string sectionName = "HttpsRedirection")
        {
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            var enableHttpsRedirection = configuration.GetValue<bool>(sectionName);
            if (enableHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }

            return app;
        }
    }
}
