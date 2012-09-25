Picross
=======

This is a free (as in beer/love/speech) and open
version of Picross.  My interest in it was mainly
the solver, so it's a rough user experience.  I
will probably fix it up later to make it more slick
for the player.

What is it?
-----------

Picross is an old puzzle which has been known by
many names (notably Nonograms) and released by many
different entities, but the idea of the puzzle isn't,
and can't be, owned by anyone in particular.  At
least, anyone who's still alive, I guess.

Rules to the game are fairly simple, but the best way
to figure them out is just to play it.  To that end,
I'd recommend a more finished game, since this one
lacks (among other things) any kind of help.

Alternately, Wikipedia has a nice article that can
help you get started.

Where do the puzzles come from?
-------------------------------

The puzzles are generated randomly by generating a
random board (independently determine if each square
is filled or not), then determining the hints which
would correspond to that board, then clearing the board.

This algorithm has two advantages:
1) It's easy to describe and implement
2) There is always (some) solution

But one enormous disadvantage
3) There can be (and usually are) multiple solutions

Which is unacceptable.  I'm interested in a better
generation algorithm, which can fix this issue, but
I haven't thought of one yet.

How are they (automatically) solved?
------------------------------------

The current solution method is to (for each row and each
column) enumerate every possible way to satisfy the hints
for that row/column which agree with the known squares.
Then if every possible solution agrees at a specific point,
that point is filled in with the agreed-on value (either
filled or certain space).

Thus far, every board I've noticed that has a unique solution
can be solved by this method, but I'm not sure whether this
is certainly reliable.

I would like a better method (or a proof that this method
always works, which I doubt), but I will not implement a
"guess and check" method, because it's boring and doesn't
really need any understanding of the problem.

It should be noted that efficiency is not an interesting
issue to raise, since solving Nonograms is (apparently)
NP-complete.

Licensing?
----------

This work (code and art assets, such as they are) are
released under the current version of the CC-BY license,
which is the most free license offered by the Creative
Commons foundation.