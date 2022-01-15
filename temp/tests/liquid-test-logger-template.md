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
{%- assign set = run.result_sets | first -%}
{%- assign test_dll = set.source | path_split | last -%}
{%- assign failed_set_results = set.results  | where: "outcome", "Failed" -%}
{%- assign skipped_set_results = set.results  | where: "outcome", "Skipped" -%}
{%- assign passed_set_results = set.results  | where: "outcome", "Passed" -%}
<details>
<summary><strong>{{overall}} - {{ test_dll }} on {{ parameters.TargetFramework }}</strong></summary>

----

## Run Summary

<p>
<strong>Overall Result:</strong> {{overall}} <br />
<strong>Pass Rate:</strong> {{pass_percentage}}% <br />
<strong>Total Tests:</strong> {{total}} <br />
<p></p>
<strong>Date:</strong> {{ run.started | local_time | date: '%Y-%m-%d %H:%M:%S' }} - {{ run.finished | local_time | date: '%Y-%m-%d %H:%M:%S' }} <br />
<strong>Run Duration:</strong> {{ run.elapsed_time_in_running_tests | format_duration }} <br />
<p></p>
<strong>GitHub Runner OS:</strong> {{ parameters.matrixOs }} <br />
<strong>Operating System:</strong> {{ parameters.os }} <br />
<strong>Framework:</strong> {{ parameters.TargetFramework }} <br />
<strong>Assembly:</strong> {{ test_dll }} <br />
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

{%- if failed_set_results.size > 0 or skipped_set_results.size > 0 -%}

## Run results

{%- if failed_set_results.size > 0 -%}
<details>
<summary>❌ Failed tests</summary>
<table>
<thead>
<tr>
<th>Test</th>
<th>Duration</th>
</tr>
</thead>
{%- for result in failed_set_results -%}
<tr>
<td>
<details>
<summary>
❌ {{ result.test_case.display_name }}
</summary>
{%- assign fully_qualified_name_splits = result.test_case.fully_qualified_name | split: "." -%}
{%- assign class_index = fully_qualified_name_splits.size | minus: 1 -%}
{%- for name_split in fully_qualified_name_splits -%}
{%- if forloop.index == class_index -%}
{%- assign test_class = name_split -%}
{%- endif -%}
{%- endfor -%}
Class:
<blockquote>{{- test_class -}}</blockquote>
Source:
<blockquote>{{- result.test_case.fully_qualified_name -}}</blockquote>
Message:
<blockquote>{{result.error_message}}</blockquote>
Stack Trace:
<blockquote>{{result.error_stack_trace}}<blockquote>
</details>
</td>
<td>{{ result.duration | format_duration }}</td>
</tr>
{%- endfor -%}
</tbody>
</table>
</details>
{%- endif -%}
{%- if skipped_set_results.size > 0 -%}
<details>
<summary>⚠️ Skipped tests</summary>
<table>
<thead>
<tr>
<th>Test</th>
<th>Duration</th>
</tr>
</thead>
{%- for result in skipped_set_results -%}
<tr>
<td>
<details>
<summary>
⚠️ {{ result.test_case.display_name }}
</summary>
{%- assign fully_qualified_name_splits = result.test_case.fully_qualified_name | split: "." -%}
{%- assign class_index = fully_qualified_name_splits.size | minus: 1 -%}
{%- for name_split in fully_qualified_name_splits -%}
{%- if forloop.index == class_index -%}
{%- assign test_class = name_split -%}
{%- endif -%}
{%- endfor -%}
Class:
<blockquote>{{- test_class -}}</blockquote>
Source:
<blockquote>{{- result.test_case.fully_qualified_name -}}</blockquote>
</details>
</td>
<td>{{ result.duration | format_duration }}</td>
</tr>
{%- endfor -%}
</tbody>
</table>
</details>
{%- endif -%}
{%- comment -%}
Commenting out the passed tests group because otherwise the test results output starts to become too long
to be used as a PR comment. Max size for a PR comment is 65 536.
Leaving the template here in case I change my mind.
{%- if passed_set_results.size > 0 -%}
<details>
<summary>✔️ Passed tests</summary>
<table>
<thead>
<tr>
<th>Test</th>
<th>Duration</th>
</tr>
</thead>
{%- for result in passed_set_results -%}
<tr>
<td>
<details>
<summary>
✔️ {{ result.test_case.display_name }}
</summary>
{%- assign fully_qualified_name_splits = result.test_case.fully_qualified_name | split: "." -%}
{%- assign class_index = fully_qualified_name_splits.size | minus: 1 -%}
{%- for name_split in fully_qualified_name_splits -%}
{%- if forloop.index == class_index -%}
{%- assign test_class = name_split -%}
{%- endif -%}
{%- endfor -%}
Class:
<blockquote>{{- test_class -}}</blockquote>
Source:
<blockquote>{{- result.test_case.fully_qualified_name -}}</blockquote>
</details>
</td>
<td>{{ result.duration | format_duration }}</td>
</tr>
{%- endfor -%}
</tbody>
</table>
</details>
{%- endif -%}
{%- endcomment -%}
{%- endif -%}

## Run Messages

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

----

</details>
