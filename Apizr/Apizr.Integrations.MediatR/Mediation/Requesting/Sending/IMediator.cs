using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Apizr.Mapping;
using MediatR;

namespace Apizr.Mediation.Requesting.Sending
{
    /// <summary>
    /// <see cref="IMediator"/> but dedicated to <see cref="TWebApi"/>, getting all shorter
    /// </summary>
    /// <typeparam name="TWebApi">The api interface to play with mediation</typeparam>
    public interface IMediator<TWebApi>
    {
        /// <summary>
        /// Send an api call command to Apizr with MediatR
        /// </summary>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <returns></returns>
        Task SendFor(Expression<Func<TWebApi, Task>> executeApiMethod);

        /// <summary>
        /// Send a cancellable api call command to Apizr with MediatR
        /// </summary>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task SendFor(Expression<Func<CancellationToken, TWebApi, Task>> executeApiMethod,
            CancellationToken token = default);

        /// <summary>
        /// Send an api call query to Apizr with MediatR
        /// </summary>
        /// <typeparam name="TApiResponse">The api response</typeparam>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <returns></returns>
        Task<TApiResponse> SendFor<TApiResponse>(Expression<Func<TWebApi, Task<TApiResponse>>> executeApiMethod);

        /// <summary>
        /// Send a cancellable api call query to Apizr with MediatR
        /// </summary>
        /// <typeparam name="TApiResponse">The api response</typeparam>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task<TApiResponse> SendFor<TApiResponse>(
            Expression<Func<CancellationToken, TWebApi, Task<TApiResponse>>> executeApiMethod,
            CancellationToken token = default);

        /// <summary>
        /// Send an api call query to Apizr with MediatR returning a mapped result
        /// </summary>
        /// <typeparam name="TModelResponse">The mapped model response</typeparam>
        /// <typeparam name="TApiResponse">The api response</typeparam>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <returns></returns>
        Task<TModelResponse> SendFor<TModelResponse, TApiResponse>(
            Expression<Func<TWebApi, Task<TApiResponse>>> executeApiMethod);

        /// <summary>
        /// Send a cancellable api call query to Apizr with MediatR returning a mapped result
        /// </summary>
        /// <typeparam name="TModelResponse">The mapped model response</typeparam>
        /// <typeparam name="TApiResponse">The api response</typeparam>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task<TModelResponse> SendFor<TModelResponse, TApiResponse>(
            Expression<Func<CancellationToken, TWebApi, Task<TApiResponse>>> executeApiMethod,
            CancellationToken token = default);

        /// <summary>
        /// Send a mapped api call query to Apizr with MediatR returning a mapped result
        /// </summary>
        /// <typeparam name="TModelResponse">The mapped model response</typeparam>
        /// <typeparam name="TApiResponse">The api response</typeparam>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <returns></returns>
        Task<TModelResponse> SendFor<TModelResponse, TApiResponse>(
            Expression<Func<TWebApi, IMappingHandler, Task<TApiResponse>>> executeApiMethod);

        /// <summary>
        /// Send a cancellable mapped api call query to Apizr with MediatR returning a mapped result
        /// </summary>
        /// <typeparam name="TModelResponse">The mapped model response</typeparam>
        /// <typeparam name="TApiResponse">The api response</typeparam>
        /// <param name="executeApiMethod">The <see cref="TWebApi"/> call to execute</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task<TModelResponse> SendFor<TModelResponse, TApiResponse>(
            Expression<Func<CancellationToken, TWebApi, IMappingHandler, Task<TApiResponse>>> executeApiMethod,
            CancellationToken token = default);
    }
}