{%- assign passed = run.test_run_statistics.passed_count -%}
{%- assign failed = run.test_run_statistics.failed_count -%}
{%- assign skipped = run.test_run_statistics.skipped_count -%}
{%- assign total = run.test_run_statistics.executed_tests_count -%}
{%- assign pass_percentage = passed | divided_by: total | times: 100.0 | round: 2  *-%}
{%- assign failed_percentage = failed | divided_by: total | times: 100.0 | round: 2  *-%}
{%- assign skipped_percentage = skipped | divided_by: total | times: 100.0 | round: 2  *-%}
{%- assign information =  run.messages | where: "level", "Informational" -%}
{%- assign warnings =  run.messages | where: "level", "Warning" -%}
{%- assign errors =  run.messages | where: "level", "Error" -%}
{%- if passed == total -%}
{%- assign overall = "✔️ Pass" *-%}
{%- elsif failed == 0 -%}
{%- assign overall = "⚠️ Indeterminate" *-%}
{%- else -%}
{%- assign overall = "❌ Fail" *-%}
{%- endif -%}

{% assign first_result_set = run.result_sets | first %}
{% assign test_dll = first_result_set.source | path_split | last %}
# {{overall}} - {{ parameters.os }} test run for {{ test_dll }} on {{ parameters.TargetFramework }}
### Run Summary

<p>
<strong>Overall Result:</strong> {{overall}} <br />
<strong>Pass Rate:</strong> {{pass_percentage}}% <br />
<strong>Run Duration:</strong> {{ run.elapsed_time_in_running_tests | format_duration }} <br />
<strong>Date:</strong> {{ run.started | local_time | date: '%Y-%m-%d %H:%M:%S' }} - {{ run.finished | local_time | date: '%Y-%m-%d %H:%M:%S' }} <br />
<strong>Operating System:</strong> {{ parameters.os }} <br />
<strong>Framework:</strong> {{ parameters.TargetFramework }} <br />
<strong>Total Tests:</strong> {{total}} <br />
</p>

<table>
<thead>
<tr>
<th>✔️ Passed</th>
<th>❌ Failed</th>
<th>⚠️ Skipped</th>
</tr>
</thead>
<tbody>
<tr>
<td>{{passed}}</td>
<td>{{failed}}</td>
<td>{{skipped}}</td>
</tr>
<tr>
<td>{{pass_percentage}}%</td>
<td>{{failed_percentage}}%</td>
<td>{{skipped_percentage}}%</td>
</tr>
</tbody>
</table>

### Result Sets
{%- for set in run.result_sets -%}
#### {{ set.source | path_split | last }} - {{set.passed_count | divided_by: set.executed_tests_count | times: 100.0 | round: 2 }}%

{% assign failed_set_results = set.results  | where: "outcome", "Failed" %}
{%- if failed_set_results.size > 0 -%}
<details>
<summary>Failed tests</summary>
<table style="white-space:nowrap;">
<thead>
<tr>
<th>Result</th>
<th>Duration</th>
<th>Traits</th>
<th>Test</th>
<th>Test class</th>
</tr>
</thead>
{%- for result in failed_set_results -%}
<tr>
<td> {% case result.outcome %} {% when 'Passed' %}✔️{% when 'Failed' %}❌{% else %}⚠️{% endcase %} {{ result.outcome }} </td>
<td> {{ result.duration | format_duration }}</td>
<td>
{%- for trait in result.test_case.traits -%}
[{{ trait.Name }} : <strong>{{ trait.Value }}</strong>]</br>
{%- endfor -%}
</td>
<td> {{ result.test_case.display_name }}
{%- if result.outcome == 'Failed' -%}
<blockquote><details>
<summary>Error</summary>
<strong>Message:</strong>
<pre><code>{{result.error_message}}</code></pre>
<strong>Stack Trace:</strong>
<pre><code>{{result.error_stack_trace}}</code></pre>
</details></blockquote>
{%- endif -%}
</td>
{% assign fully_qualified_name_splits = result.test_case.fully_qualified_name | split: "." %}
{% assign class_index = fully_qualified_name_splits.size | minus: 1 %}
{%- for name_split in fully_qualified_name_splits -%}
{%- if forloop.index == class_index -%}
{%- assign test_class = name_split -%}
{%- endif -%}
{%- endfor -%}
<td>{{ test_class }}
<blockquote><details>
<summary>Fully qualified name</summary>
{{ result.test_case.fully_qualified_name }}
</details></blockquote>
</tr>
{%- endfor -%}
</tbody>
</table>
</details>
{%- endif -%}

{% assign skipped_set_results = set.results  | where: "outcome", "Skipped" %}
{%- if skipped_set_results.size > 0 -%}
<details>
<summary>Skipped tests</summary>
<table style="white-space:nowrap;">
<thead>
<tr>
<th>Result</th>
<th>Duration</th>
<th>Traits</th>
<th>Test</th>
<th>Test class</th>
</tr>
</thead>
{%- for result in skipped_set_results -%}
<tr>
<td> {% case result.outcome %} {% when 'Passed' %}✔️{% when 'Failed' %}❌{% else %}⚠️{% endcase %} {{ result.outcome }} </td>
<td>{{ result.duration | format_duration }}</td>
<td>
{%- for trait in result.test_case.traits -%}
[{{ trait.Name }} : <strong>{{ trait.Value }}</strong>]</br>
{%- endfor -%}
</td>
<td> {{ result.test_case.display_name }}
{%- if result.outcome == 'Failed' -%}
<blockquote><details>
<summary>Error</summary>
<strong>Message:</strong>
<pre><code>{{result.error_message}}</code></pre>
<strong>Stack Trace:</strong>
<pre><code>{{result.error_stack_trace}}</code></pre>
</details></blockquote>
{%- endif -%}
</td>
{% assign fully_qualified_name_splits = result.test_case.fully_qualified_name | split: "." %}
{% assign class_index = fully_qualified_name_splits.size | minus: 1 %}
{%- for name_split in fully_qualified_name_splits -%}
{%- if forloop.index == class_index -%}
{%- assign test_class = name_split -%}
{%- endif -%}
{%- endfor -%}
<td>{{ test_class }}
<blockquote><details>
<summary>Fully qualified name</summary>
{{ result.test_case.fully_qualified_name }}
</details></blockquote>
</tr>
{%- endfor -%}
</tbody>
</table>
</details>
{%- endif -%}

{% assign passed_set_results = set.results  | where: "outcome", "Passed" %}
{%- if passed_set_results.size > 0 -%}
<details>
<summary>Passed tests</summary>
<table style="white-space:nowrap;">
<thead>
<tr>
<th>Result</th>
<th>Duration</th>
<th>Traits</th>
<th>Test</th>
<th>Test class</th>
</tr>
</thead>
{%- for result in passed_set_results -%}
<tr>
<td> {% case result.outcome %} {% when 'Passed' %}✔️{% when 'Failed' %}❌{% else %}⚠️{% endcase %} {{ result.outcome }} </td>
<td>{{ result.duration | format_duration }}</td>
<td>
{%- for trait in result.test_case.traits -%}
[{{ trait.Name }} : <strong>{{ trait.Value }}</strong>]</br>
{%- endfor -%}
</td>
<td> {{ result.test_case.display_name }}
{%- if result.outcome == 'Failed' -%}
<blockquote><details>
<summary>Error</summary>
<strong>Message:</strong>
<pre><code>{{result.error_message}}</code></pre>
<strong>Stack Trace:</strong>
<pre><code>{{result.error_stack_trace}}</code></pre>
</details></blockquote>
{%- endif -%}
</td>
{% assign fully_qualified_name_splits = result.test_case.fully_qualified_name | split: "." %}
{% assign class_index = fully_qualified_name_splits.size | minus: 1 %}
{%- for name_split in fully_qualified_name_splits -%}
{%- if forloop.index == class_index -%}
{%- assign test_class = name_split -%}
{%- endif -%}
{%- endfor -%}
<td>{{ test_class }}
<blockquote><details>
<summary>Fully qualified name</summary>
{{ result.test_case.fully_qualified_name }}
</details></blockquote>
</tr>
{%- endfor -%}
</tbody>
</table>
</details>
{%- endif -%}

{%- endfor -%}

### Run Messages
<details>
<summary>Informational</summary>
<pre><code>
{%- for message in information -%}
{{ message.message }}
{%- endfor -%}
</code></pre>
</details>

{%- if warnings.size > 0 -%}
<details>
<summary>Warning</summary>
<pre><code>
{%- for message in warnings -%}
{{message.message}}
{%- endfor -%}
</code></pre>
</details>
{%- endif -%}

{%- if errors.size > 0 -%}
<details>
<summary>Error</summary>
<pre><code>
{%- for message in errors -%}
{{message.message}}
{%- endfor -%}
</code></pre>
</details>
{%- endif -%}
