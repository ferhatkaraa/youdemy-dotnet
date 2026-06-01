document.addEventListener("DOMContentLoaded", () => {
    // Setup interactive syllabus checklist checkboxes
    const checkboxes = document.querySelectorAll(".js-progress-checkbox");
    
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener("click", async (e) => {
            e.stopPropagation(); // Avoid triggering parent element events (e.g., redirecting)
            
            const lessonId = checkbox.dataset.lessonId;
            const isCompletedNow = !checkbox.classList.contains("checked");
            
            // Set temporary loading visual state
            checkbox.style.opacity = "0.5";
            checkbox.style.pointerEvents = "none";
            
            try {
                const response = await fetch("/Lessons/ToggleComplete", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        // Send anti-forgery token if present
                        "RequestVerificationToken": getCsrfToken()
                    },
                    body: `lessonId=${lessonId}&isCompleted=${isCompletedNow}`
                });
                
                if (response.ok) {
                    const data = await response.json();
                    
                    if (data.success) {
                        // 1. Toggle checkbox visual state
                        if (isCompletedNow) {
                            checkbox.classList.add("checked");
                        } else {
                            checkbox.classList.remove("checked");
                        }
                        
                        // 2. Toggle parent lesson list item completion style (e.g., line-through text)
                        const parentItem = document.getElementById(`syllabus-item-${lessonId}`);
                        if (parentItem) {
                            if (isCompletedNow) {
                                parentItem.classList.add("completed");
                            } else {
                                parentItem.classList.remove("completed");
                            }
                        }
                        
                        // 3. Update the global progress metrics text and progress bar fill
                        updateProgressBar(data.completedCount, data.totalCount, data.progressPercentage);
                    } else {
                        console.error("Progress update failed:", data.message);
                        alert(data.message || "İlerleme kaydedilirken bir hata oluştu.");
                    }
                } else if (response.status === 401) {
                    alert("Lütfen ilerlemeyi kaydetmek için giriş yapın.");
                    window.location.href = "/Auth/Login";
                } else {
                    console.error("Network response was not OK");
                    alert("Ağ bağlantısında bir hata oluştu.");
                }
            } catch (err) {
                console.error("AJAX Error:", err);
                alert("İstek gönderilirken beklenmeyen bir hata oluştu.");
            } finally {
                // Restore loading states
                checkbox.style.opacity = "1";
                checkbox.style.pointerEvents = "auto";
            }
        });
    });
    
    // Function to parse the CSRF Verification Token from forms
    function getCsrfToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : "";
    }
    
    // Function to update progress sidebar UI dynamically
    function updateProgressBar(completed, total, percentage) {
        const progressCountEl = document.getElementById("js-progress-count");
        const progressPercentageEl = document.getElementById("js-progress-percentage");
        const progressBarFillEl = document.getElementById("js-progress-bar-fill");
        
        if (progressCountEl) {
            progressCountEl.textContent = `${completed}/${total} Tamamlandı`;
        }
        
        if (progressPercentageEl) {
            progressPercentageEl.textContent = `${percentage}%`;
        }
        
        if (progressBarFillEl) {
            progressBarFillEl.style.width = `${percentage}%`;
        }
    }

    // Setup Theme Toggling Logic
    const themeToggleBtn = document.getElementById("js-theme-toggle");
    if (themeToggleBtn) {
        const sunIcon = themeToggleBtn.querySelector(".theme-icon-sun");
        const moonIcon = themeToggleBtn.querySelector(".theme-icon-moon");
        
        // Read active theme (initialized by head script)
        const currentTheme = document.documentElement.getAttribute("data-theme") || "dark";
        updateThemeIcons(currentTheme);
        
        themeToggleBtn.addEventListener("click", () => {
            const activeTheme = document.documentElement.getAttribute("data-theme");
            const targetTheme = activeTheme === "light" ? "dark" : "light";
            
            document.documentElement.setAttribute("data-theme", targetTheme);
            localStorage.setItem("youdemy-theme", targetTheme);
            updateThemeIcons(targetTheme);
        });
        
        function updateThemeIcons(theme) {
            if (theme === "light") {
                sunIcon.style.display = "none";
                moonIcon.style.display = "block";
            } else {
                sunIcon.style.display = "block";
                moonIcon.style.display = "none";
            }
        }
    }
});
