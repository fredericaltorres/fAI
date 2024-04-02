https://platform.openai.com/docs/guides/prompt-engineering/six-strategies-for-getting-better-results
https://www.almabetter.com/bytes/tutorials/artificial-intelligence/heuristic-function-in-ai

As logic analyst, analyze the following variables in triple quotes and answer the question in a JSON object with a property 'answer':
"""
variable A is set to true.
variable B is set to true.
variable C is set to true.
variable D is set to false.

If variable A is true and variable B is true then SET variable C to true.
If variable B is true and variable C is true then SET variable D to true.

Evaluate all the rules above and answer the question:
Is the variable D true ?
"""

1 ----------------------------------------------

SYSTEM:
Use the following step-by-step instructions to respond to user inputs.
Step 1 - The user will provide you with text in triple quotes.  
         Evaluate all the rules above with explanation.
Step 2 - Answer the question:
Is the variable D true  with explanation?

USER:
"""
variable A is set to true.
variable B is set to true.
variable C is set to true.
variable D is set to false.

SET variable C to true, If variable A is true and variable B is true.
SET variable D to true, If variable B is true and variable C is true.
"""



2 ----------------------------------------------

SYSTEM:
Use the following step-by-step instructions to respond to user inputs.
Step 1 - The user will provide you with text in triple quotes.  
         Evaluate all the rules above with explanation.
Step 2 - Answer the question:
Is the variable F true  with explanation?

USER:
"""
variable A is set to true.
variable B is set to true.
variable C is set to true.
variable D is set to false.
variable E is set to true.
variable F is set to false.

SET variable C to true, If variable A is true and variable B is true.
SET variable D to true, If variable B is true and variable C is true.
SET variable F to true, If variable A,B,C,D and E are true.
"""