window.downloadFile = (url) => {
    const link = document.createElement("a");
    link.href = url;
    link.target = "_blank";
    link.click();
};