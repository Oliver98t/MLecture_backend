namespace Common;

public static class GroqAPIConsts
{
    public static string testResult = @"**Balanced Forces (Equilibrium)**

- A system is in equilibrium when the **net external force** on it is zero.
- No net force ⇒ no acceleration, constant momentum and constant velocity.
- To change the velocity or momentum a **resultant force** must be applied (Newton’s 2nd law: $\mathbf{F}_{\text{net}} = m\mathbf{a}$).

---

**Resultant of Perpendicular Forces**

When two forces are at right angles, the resultant magnitude is found with the Pythagorean theorem:

$$
F_{\text{R}} = \sqrt{F_{1}^{2}+F_{2}^{2}}
$$

*Example:* $F_{1}=5\ \text{N}$ (downward), $F_{2}=10\ \text{N}$ (rightward)

$$
F_{\text{R}} = \sqrt{5^{2}+10^{2}} = 11.2\ \text{N}
$$

The third force required for equilibrium is equal in magnitude and opposite in direction to $F_{\text{R}}$.

---

**Resolving a Non‑Perpendicular Force**

If forces are not orthogonal, resolve each into horizontal (x) and vertical (y) components.
For a force $F$ at angle $\theta$ measured from the vertical:

- Horizontal component: $F_{x}=F\sin\theta$
- Vertical component: $F_{y}=F\cos\theta$

*(Use the “turn away from the angle → sin” rule: when you turn away from the given angle to obtain the component, use $\sin$; when you turn through the angle, use $\cos$.)*

*Worked example:*

- $F_{1}=20\ \text{N}$ downward (pure $y$).
- $F_{2}=15\ \text{N}$ at $35^{\circ}$ to the vertical.

Components of $F_{2}$:

$$
F_{2x}=15\sin 35^{\circ}=8.6\ \text{N},\qquad
F_{2y}=15\cos 35^{\circ}=12.3\ \text{N}
$$

Combine vertical components (down positive):

$$
F_{y,\text{net}} = 20\ \text{N} - 12.3\ \text{N}=7.7\ \text{N}
$$

Horizontal component is $F_{x}=8.6\ \text{N}$.

Resultant of these orthogonal components:

$$
F_{\text{R}} = \sqrt{(8.6)^{2}+(7.7)^{2}} = 11.5\ \text{N}
$$

A third force of $11.5\ \text{N}$ opposite to this resultant restores equilibrium.

---

**Tension in Two Supporting Cables**

A mass $m$ is suspended by two cables with tensions $T_{1}$ and $T_{2}$ making angles $\theta_{1}$ and $\theta_{2}$ to the **horizontal**.

- Weight: $W = mg$ (downward).
- Vertical equilibrium:

$$
T_{1}\sin\theta_{1}+T_{2}\sin\theta_{2}=mg \tag{1}
$$

- Horizontal equilibrium:

$$
T_{1}\cos\theta_{1}=T_{2}\cos\theta_{2} \tag{2}
$$

From (2):

$$
T_{1}=T_{2}\,\frac{\cos\theta_{2}}{\cos\theta_{1}}
$$

Insert into (1) and solve for $T_{2}$:

$$
T_{2}\left(\frac{\cos\theta_{2}}{\cos\theta_{1}}\sin\theta_{1}+\sin\theta_{2}\right)=mg
$$

Using $\displaystyle \frac{\sin\theta}{\cos\theta}=\tan\theta$ simplifies the bracket.
Finally, obtain $T_{2}$, then use (2) to find $T_{1}$.

---

**Numerical Example – Gymnast on Two Cables**

- Mass: $m=90\ \text{kg}$ → $W=mg=90(9.8)=882\ \text{N}$.
- Angles: $\theta_{1}=40^{\circ}$, $\theta_{2}=30^{\circ}$ (to the horizontal).

Vertical equilibrium (1):

$$
T_{1}\sin40^{\circ}+T_{2}\sin30^{\circ}=882 \tag{A}
$$

Horizontal equilibrium (2):

$$
T_{1}\cos40^{\circ}=T_{2}\cos30^{\circ} \;\;\Longrightarrow\;\;
T_{1}=T_{2}\frac{\cos30^{\circ}}{\cos40^{\circ}} \tag{B}
$$

Substitute (B) into (A):

$$
T_{2}\left(\frac{\cos30^{\circ}}{\cos40^{\circ}}\sin40^{\circ}+\sin30^{\circ}\right)=882
$$

Compute the bracket (≈ 1.23):

$$
1.23\,T_{2}=882 \;\;\Longrightarrow\;\; T_{2}\approx719\ \text{N}
$$

Then

$$
T_{1}=719\frac{\cos30^{\circ}}{\cos40^{\circ}}\approx813\ \text{N}
$$

Thus the cables must each sustain roughly $T_{1}=813\ \text{N}$ and $T_{2}=719\ \text{N}$ to keep the gymnast in equilibrium.

---

**Key Takeaways**

1. **Equilibrium** ⇔ net force zero (both horizontal and vertical components).
2. Resolve any angled force into orthogonal components before combining.
3. Use the Pythagorean theorem for the resultant of perpendicular components.
4. For multiple cables, write separate **vertical** and **horizontal** balance equations, then solve the simultaneous system.
5. Practise with varied configurations to become fluent in vector resolution.  ";
}

public static class GlobalConsts
{
    public static string? LocalEnvironment = Environment.GetEnvironmentVariable("LocalEnvironment");
    public static string? GroqAPIEnable = Environment.GetEnvironmentVariable("GroqAPIEnable");
    public static string? TranscriptAPIEnable = Environment.GetEnvironmentVariable("TranscriptAPIEnable");
}

public static class QueueConsts
{
    public const string triggerSuccessMessage = "ready";
    public const string triggerFailMessage = "Not ready";
    public const string transcriptionJob = "transcription-job";
    public const string transcriptionResults = "transcribe-results";
}

public static class TableConsts
{
    public const string transcriptionTable = "transcriptions";
    public const string notesTable = "notes";
    public const string userTable = "users";
}

public class StartCreateNotesQueueData
{
    public string User { get; set; }
    public string Url { get; set; }
    public string JobId { get; set; }

    public StartCreateNotesQueueData(string user, string url, string jobId)
    {
        User = user;
        Url = url;
        JobId = jobId;
    }
}

public class TranscribeTriggerQueueData
{
    public string User {get; set;}
    public string Transcript {get; set;}
    public string JobId {get; set;}

    public TranscribeTriggerQueueData(string user, string transcript, string jobId)
    {
        User = user;
        Transcript = transcript;
        JobId = jobId;
    }
}