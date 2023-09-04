# Mocking HttpClient's responses using in-process vs out-of-process

Both the [in-process http response mocking method](./http-mocking-in-process.md) and the [out-of-process http response mocking method](./http-mocking-out-of-process.md) allow you to control the responses for outgoing http requests done via the `HttpClient` class.

Which one to use is really up to your preference. I will however point some differences:

- When using out-of-process you can keep the configuration code for your HttpMockServer more cleanly separated from your tests. This however comes at the cost of creating a `Startup` class for your tests.

- If you require more complex configuration for `HttpMockServer` you should use the out-of-process mocking because it gives you access to the full power of `asp.net` when configuring the server.

- When using in-process mocking you HAVE to know if the outgoing request you are mocking is going to be performed by a basic, named or typed instance of the `HttpClient` as each one requires a different kind of mocking. If you are using out-of-process you don't have to care about that.

- The in-process mocking will replace the `HttpMessageHandler` used by the `HttpClient` to do the outgoing requests and as such no network call actually takes place during the test. The out-of-process mocking will start a server on `localhost` and the test must be configured so that the `HttpClient` sends the request to it. In this case a network call will happen. Given this it might be easier to test more complex resilience scenarios (timeouts, throtling, etc) using the out-of-process mocking.
