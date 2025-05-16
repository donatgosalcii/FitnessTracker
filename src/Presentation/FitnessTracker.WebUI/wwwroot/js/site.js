const themeToggle = document.getElementById('theme-toggle');
const themeIcon = themeToggle.querySelector('i');
const prefersDarkScheme = window.matchMedia('(prefers-color-scheme: dark)');

function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
    
    if (theme === 'dark') {
        themeIcon.classList.remove('fa-moon');
        themeIcon.classList.add('fa-sun');
    } else {
        themeIcon.classList.remove('fa-sun');
        themeIcon.classList.add('fa-moon');
    }
}

function initializeTheme() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        setTheme(savedTheme);
    } else if (prefersDarkScheme.matches) {
        setTheme('dark');
    } else {
        setTheme('light');
    }
}

themeToggle.addEventListener('click', () => {
    const currentTheme = document.documentElement.getAttribute('data-theme');
    setTheme(currentTheme === 'dark' ? 'light' : 'dark');
});

prefersDarkScheme.addEventListener('change', (e) => {
    const savedTheme = localStorage.getItem('theme');
    if (!savedTheme) {
        setTheme(e.matches ? 'dark' : 'light');
    }
});

document.addEventListener('DOMContentLoaded', initializeTheme);

document.addEventListener('DOMContentLoaded', function() {
    const chatBubble = document.getElementById('chat-bubble');
    const chatWindow = document.getElementById('chat-window');
    
    if (!chatBubble || !chatWindow) {
        return;
    }
    
    const minimizeChat = document.getElementById('minimize-chat');
    const messageInput = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');
    const chatMessages = document.getElementById('chat-messages');
    
    const isChatOpen = localStorage.getItem('chatOpen') === 'true';
    if (isChatOpen) {
        chatWindow.classList.add('active');
    }
    
    chatBubble.addEventListener('click', function() {
        chatWindow.classList.add('active');
        messageInput.focus();
        localStorage.setItem('chatOpen', 'true');
        
        if (chatMessages.children.length === 0) {
            loadChatHistory();
        }
    });
    
    minimizeChat.addEventListener('click', function() {
        chatWindow.classList.remove('active');
        localStorage.setItem('chatOpen', 'false');
    });
    
    sendButton.addEventListener('click', sendMessage);
    messageInput.addEventListener('keypress', function(e) {
        if (e.which === 13 && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
    
    if (isChatOpen && chatMessages.children.length === 0) {
        loadChatHistory();
    }
    
    function formatTimestamp(dateString) {
        const date = new Date(dateString);
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    
    async function sendMessage() {
        const message = messageInput.value.trim();
        if (!message) return;
        
        const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        messageInput.value = '';
        
        const now = new Date();
        addMessage(message, true, now);
        
        const loadingIndicator = document.createElement('div');
        loadingIndicator.className = 'message text-start loading-message';
        loadingIndicator.innerHTML = '<div class="message-bubble"><div class="typing-indicator"><span></span><span></span><span></span></div></div>';
        chatMessages.appendChild(loadingIndicator);
        chatMessages.scrollTop = chatMessages.scrollHeight;
        
        sendButton.disabled = true;
        
        try {
            const response = await fetch('/api/Chat/send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiforgeryToken
                },
                body: JSON.stringify(message)
            });
            
            if (loadingIndicator.parentNode) {
                loadingIndicator.parentNode.removeChild(loadingIndicator);
            }
            
            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.error || 'Failed to send message');
            }
            
            const data = await response.json();
            addMessage(data.response, false, data.timestamp);
        } catch (error) {
            console.error('Error:', error);
            
            if (loadingIndicator.parentNode) {
                loadingIndicator.parentNode.removeChild(loadingIndicator);
            }
            
            addMessage('Error: ' + (error.message || 'An unexpected error occurred. Please try again.'), false, new Date());
        } finally {
            sendButton.disabled = false;
        }
    }
    
    function addMessage(message, isUser = false, timestamp = new Date()) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${isUser ? 'text-end' : 'text-start'}`;
        
        const messageBubble = document.createElement('div');
        messageBubble.className = 'message-bubble';
        messageBubble.textContent = message;
        
        const timestampDiv = document.createElement('div');
        timestampDiv.className = 'chat-timestamp';
        
        const displayTime = timestamp instanceof Date ? 
            timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : 
            formatTimestamp(timestamp);
            
        timestampDiv.textContent = displayTime;
        
        messageDiv.appendChild(messageBubble);
        messageDiv.appendChild(timestampDiv);
        chatMessages.appendChild(messageDiv);
        
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
    
    async function loadChatHistory() {
        if (!chatMessages) return;
        
        chatMessages.innerHTML = '<div class="text-center p-3"><div class="spinner-border spinner-border-sm text-primary" role="status"><span class="visually-hidden">Loading...</span></div> Loading messages...</div>';
        
        const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        try {
            const response = await fetch('/api/Chat/history', {
                headers: {
                    'RequestVerificationToken': antiforgeryToken
                }
            });
            
            if (!response.ok) {
                throw new Error('Failed to load chat history');
            }
            
            const history = await response.json();
            
            chatMessages.innerHTML = '';
            
            if (history.length === 0) {
                addMessage('👋 Hello! I\'m your fitness assistant. How can I help you today? Ask me about workouts, nutrition, or fitness goals.', false);
            } else {
                history.forEach(msg => {
                    addMessage(msg.message, true, msg.timestamp);
                    addMessage(msg.response, false, msg.timestamp);
                });
                
                chatMessages.scrollTop = chatMessages.scrollHeight;
            }
        } catch (error) {
            console.error('Error loading chat history:', error);
            chatMessages.innerHTML = '';
            addMessage('Failed to load chat history. Please refresh to try again.', false);
        }
    }
});
