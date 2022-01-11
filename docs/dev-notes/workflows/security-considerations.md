# Security considerations on GitHub workflows

## Restrictions

GitHub Actions have restrictions on workflow runs triggered by public repository forks. The restrictions apply to the `pull_request` event triggered by a fork opening a pull request in the upstream repository.

- Events from forks cannot access secrets, except for the default `GITHUB_TOKEN`.
    > With the exception of GITHUB_TOKEN, secrets are not passed to the runner when a workflow is triggered from a forked repository.

    [GitHub Actions: Using encrypted secrets in a workflow](https://docs.github.com/en/actions/configuring-and-managing-workflows/creating-and-storing-encrypted-secrets#using-encrypted-secrets-in-a-workflow)

- The `GITHUB_TOKEN` has read-only access when an event is triggered by a forked repository.

   [GitHub Actions: Permissions for the GITHUB_TOKEN](https://docs.github.com/en/actions/configuring-and-managing-workflows/authenticating-with-the-github_token#permissions-for-the-github_token)

These restrictions mean that during a `pull_request` event triggered by a forked repository, actions have no write access to GitHub resources and will fail on any attempt.

A job condition can be added to prevent workflows from executing when triggered by a repository fork.

```yml
on: pull_request
jobs:
  example:
    runs-on: ubuntu-latest
    # Check if the event is not triggered by a fork
    if: github.event.pull_request.head.repo.full_name == github.repository
```

Private repositories can be configured to [enable workflows](https://docs.github.com/en/github/administering-a-repository/disabling-or-limiting-github-actions-for-a-repository#enabling-workflows-for-private-repository-forks) from forks to run without restriction.

## Beware of pull_request_target

An indirect vehicle for compromise is via [`pull_request_target`](https://docs.github.com/en/actions/reference/events-that-trigger-workflows#pull_request_target). This workflow trigger behaves just like its sibling, `pull_request`, except that the GitHub Actions runner runs in the context of the target repository. To be explicit, the target repository is the repository that the pull request is attempting to merge code into.

GHA runners triggered with `pull_request_target` have two properties that you need to be aware of:

1) By default the runner uses the code of the target repository, not the incoming pull request’s repository.
2) The runner uses the environment (think: secrets and write permissions) of the target repository, not the incoming pull request’s repository.

Because of (1), using `pull_request_target` is usually safe from malicious attempts to abuse (2) because it does not use the incoming pull requests’s code. However, if the runner were to explicitly check out and use the incoming pull requests’s code, then we’ve violated (1), and now we’re in dangerous territory.

There are a lot of components in play, so let's summarize:

- The `pull_request` trigger uses incoming code but does not have write permissions or access to the target repo’s secrets.
- The `pull_request_target` trigger uses the target code and does have write permissions and access to the target repo’s secrets.
- Therefore, the dangerous situation here is explicitly checking out the incoming code, because it now has write permissions and access to the target repo’s secrets.

A GHA workflow can check out the incoming code in a variety of ways, the most common of which is with the `actions/checkout` Action. If the GHA workflow uses `pull_request_target` and checks out the incoming code, we’re really flirting with disaster. The only thing needed to enable full runner compromise is a workflow step that executes any part of the incoming code. `make`, `npm install`, and `python setup.py install` are all examples of executable code which an attacker would have influence over. Simply modify one of the relevant files, make an incoming pull request, and the GHA runner is compromised.

As per [Github Actions and the threat of malicious pull requests](https://nathandavison.com/blog/github-actions-and-the-threat-of-malicious-pull-requests):

> ... if you're using the pull_request_target event in Github Actions, don't use actions/checkout to then checkout the pull request's code. If you do, then you are opening yourself up to the malicious pull request attack.
>
> If you must combine the two, then make sure you guard your configuration with conditions that only runs steps with access to secrets when the pull request being checked out in the workflow is trusted, whatever that means to you and your requirements. If you search Github, you will find configurations that use the if: feature to do something like this - be careful that your logic is not faulty and test, test, test. Use a non privileged account to fork the repo, and try and exploit it using the techniques covered.

**To be clear, not every instance of `pull_request_target` paired with checking out incoming code is a vulnerability**. If the workflow does not run any of the incoming code, then it is usually safe. However, this pair of patterns is an indicator of dangerous behavior.

**In general, don’t use `pull_request_target` unless you need to and can verify that you’re not running anything from the incoming code.**

## How to be secure

As explained by [Keeping your GitHub Actions and workflows secure: Preventing pwn requests](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/), **anything that is used on a privileged workflow context must be trusted data**. For example, binaries built from an untrusted PR, would be a security vulnerability if executed in the privileged workflow_run workflow context.

For public repositories, the recommended setup to handle pull requests from forked repositories is to use separate workflows:

1) Workflows that run in the context of the PR head branch will have a read-only token and no access to secrets. Data that you need to perform further actions should be uploaded as a workflow artifact.
2) Steps that require priviliged context should run in a separate workflow which does NOT execute the code of the forked repository. Any information required should be either available on this workflow or downloaded workflow artifacts from less priviliged workflows.

An example would be:

- I want to create a pull request comment with the results of the test run. This requires write priviliges.
- The main workflow has a `pull_request` trigger which will run the code from the forked repo on a non-priviliged context. The test results are uploaded as a workflow artifact.
- A separate workflow using the `workflow_run` trigger will start once the main workflow completes. This workflow runs in the context of the target repository with a priviliged context. It will download the test results artifact from the non-priviliged workflow and create a comment on the Pull Request.

## Further security reading

GitHub blog post series on "Keeping your GitHub Actions and workflows secure":

- [Keeping your GitHub Actions and workflows secure Part 1: Preventing pwn requests](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/)
- [Keeping your GitHub Actions and workflows secure Part 2: Untrusted input](https://securitylab.github.com/research/github-actions-untrusted-input/)
- [Keeping your GitHub Actions and workflows secure Part 3: How to trust your building blocks](https://securitylab.github.com/research/github-actions-building-blocks/)

Good security practices for using GitHub Actions features:

- [Security hardening for GitHub Actions](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
- [Github Actions and the threat of malicious pull requests](https://nathandavison.com/blog/github-actions-and-the-threat-of-malicious-pull-requests)
- [GitHub Actions improvements for fork and pull request workflows](https://github.blog/2020-08-03-github-actions-improvements-for-fork-and-pull-request-workflows/)
